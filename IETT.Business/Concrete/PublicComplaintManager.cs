using System.Data;
using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Complaints;
using IETT.Entity.Entities;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IETT.Business.Concrete
{
    public class PublicComplaintManager : IPublicComplaintService
    {
        public const int MaximumDescriptionLength = 2000;

        private readonly IETTDbContext _context;
        private readonly ILogger<PublicComplaintManager> _logger;

        public PublicComplaintManager(
            IETTDbContext context,
            ILogger<PublicComplaintManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PublicComplaintTypeDto>> GetComplaintTypesAsync()
        {
            return await _context.ComplaintTypes
                .AsNoTracking()
                .OrderBy(item => item.ComplaintTypeName)
                .Select(item => new PublicComplaintTypeDto
                {
                    Id = item.Id,
                    Name = item.ComplaintTypeName
                })
                .ToListAsync();
        }

        public async Task<PublicComplaintTrackingLookupResult> GetByTrackingCodeAsync(
            string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
                return new() { Status = PublicComplaintTrackingLookupStatus.NotFound };

            var normalizedTrackingCode = trackingCode.Trim().ToUpperInvariant();
            var complaints = await _context.Complaints
                .AsNoTracking()
                .Where(complaint => EF.Functions.Collate(
                    complaint.TrackingCode,
                    "Turkish_100_CI_AS") == normalizedTrackingCode)
                .Select(complaint => new { complaint.Id, complaint.TrackingCode })
                .Take(2)
                .ToListAsync();

            if (complaints.Count == 0)
                return new() { Status = PublicComplaintTrackingLookupStatus.NotFound };
            if (complaints.Count > 1)
                return new() { Status = PublicComplaintTrackingLookupStatus.DuplicateTrackingCode };

            var complaint = complaints[0];
            var hasOpenInvestigation = await _context.Investigations
                .AsNoTracking()
                .AnyAsync(investigation => investigation.ComplaintId == complaint.Id
                    && investigation.ProcessStatus != InvestigationProcessStatus.Completed);

            var latestCompleted = hasOpenInvestigation
                ? null
                : await _context.Investigations
                    .AsNoTracking()
                    .Where(investigation => investigation.ComplaintId == complaint.Id
                        && investigation.ProcessStatus == InvestigationProcessStatus.Completed)
                    .OrderByDescending(investigation => investigation.ClosedDate)
                    .ThenByDescending(investigation => investigation.Id)
                    .Select(investigation => new { investigation.InvestigationResult })
                    .FirstOrDefaultAsync();

            var isCompleted = !hasOpenInvestigation && latestCompleted is not null;
            return new()
            {
                Status = PublicComplaintTrackingLookupStatus.Success,
                Complaint = new PublicComplaintTrackingDto
                {
                    TrackingCode = complaint.TrackingCode,
                    Status = isCompleted ? "Tamamlandı" : "Süreç devam ediyor",
                    FinalDecision = isCompleted ? latestCompleted?.InvestigationResult : null
                }
            };
        }

        public async Task<PublicComplaintOperationResult> CreateAsync(
            CreatePublicComplaintDto dto)
        {
            var validation = Validate(dto);

            if (validation != PublicComplaintOperationStatus.Success)
            {
                return PublicComplaintOperationResult.Failure(validation);
            }

            var doorNumber = dto.DoorNumber.Trim();
            var routeCode = dto.RouteCode?.Trim();

            var vehicleIds = await _context.Vehicles
                .AsNoTracking()
                .Where(item => item.DoorNumber.ToUpper() == doorNumber.ToUpper())
                .OrderBy(item => item.Id)
                .Select(item => item.Id)
                .Take(2)
                .ToListAsync();

            if (vehicleIds.Count == 0)
                return PublicComplaintOperationResult.Failure(
                    PublicComplaintOperationStatus.VehicleNotFound);

            if (vehicleIds.Count > 1)
                return PublicComplaintOperationResult.Failure(
                    PublicComplaintOperationStatus.VehicleAmbiguous);

            var routeIds = string.IsNullOrWhiteSpace(routeCode)
                ? new List<int>()
                : await _context.BusRoutes
                .AsNoTracking()
                .Where(item => item.RouteCode.ToUpper() == routeCode.ToUpper())
                .OrderBy(item => item.Id)
                .Select(item => item.Id)
                .Take(2)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(routeCode) && routeIds.Count == 0)
                return PublicComplaintOperationResult.Failure(
                    PublicComplaintOperationStatus.RouteNotFound);

            if (routeIds.Count > 1)
                return PublicComplaintOperationResult.Failure(
                    PublicComplaintOperationStatus.RouteAmbiguous);

            if (!await _context.ComplaintTypes.AsNoTracking()
                    .AnyAsync(item => item.Id == dto.ComplaintTypeId))
                return PublicComplaintOperationResult.Failure(
                    PublicComplaintOperationStatus.ComplaintTypeNotFound);

            var vehicleId = vehicleIds[0];
            var tripIds = await FindMatchingTripIdsAsync(
                vehicleId,
                routeIds.Count == 1 ? routeIds[0] : null,
                dto.IncidentDateTime);

            if (tripIds.Count == 0)
                return PublicComplaintOperationResult.Failure(PublicComplaintOperationStatus.TripNotFound);
            if (tripIds.Count > 1)
                return PublicComplaintOperationResult.Failure(PublicComplaintOperationStatus.TripAmbiguous);

            var trip = await _context.Trips.AsNoTracking()
                .Where(item => item.Id == tripIds[0])
                .Select(item => new { item.Id, item.RouteId, item.Driver.GarageId })
                .SingleAsync();
            var inspectorId = await _context.Inspectors.AsNoTracking()
                .Where(item => item.GarageId == trip.GarageId)
                .OrderBy(item => item.Investigations.Count(x => x.ClosedDate == null))
                .ThenBy(item => item.Id)
                .Select(item => (int?)item.Id)
                .FirstOrDefaultAsync();
            if (!inspectorId.HasValue)
                return PublicComplaintOperationResult.Failure(PublicComplaintOperationStatus.InspectorNotAvailable);

            var now = DateTime.Now;
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var trackingCode = await GenerateTrackingCodeAsync(now.Year);
            var complaint = new Complaint
            {
                TrackingCode = trackingCode,
                ComplaintTypeId = dto.ComplaintTypeId,
                RouteId = trip.RouteId,
                VehicleId = vehicleId,
                StopId = null,
                TripId = trip.Id,
                ComplaintDate = dto.IncidentDateTime.Date,
                ComplaintTime = dto.IncidentDateTime.TimeOfDay,
                ComplaintDescription = dto.ComplaintDescription.Trim(),
                ComplaintStatus = ComplaintStatusEnum.Pending,
                CreatedDate = now
            };

            _context.Complaints.Add(complaint);
            try
            {
                await _context.SaveChangesAsync();
                _context.Investigations.Add(new Investigation
                {
                    ComplaintId = complaint.Id,
                    InspectorId = inspectorId.Value,
                    InvestigationTitle = $"Vatandaş Şikâyeti - {complaint.TrackingCode}",
                    InvestigationDescription = "Vatandaş tarafından oluşturulan şikâyet otomatik olarak atanmıştır.",
                    ProcessStatus = InvestigationProcessStatus.AwaitingInspectorReview,
                    CreatedDate = now
                });
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return PublicComplaintOperationResult.Success(
                new PublicComplaintCreatedDto
                {
                    Id = complaint.Id,
                    TrackingCode = complaint.TrackingCode,
                    ComplaintStatusName = "Beklemede",
                    CreatedDate = complaint.CreatedDate,
                    Message = "Şikâyetiniz başarıyla oluşturuldu. Takip kodunuzu saklayınız."
                });
        }

        private async Task TryCreateInvestigationAsync(
            Complaint complaint,
            DateTime createdDate)
        {
            if (!complaint.TripId.HasValue)
            {
                _logger.LogWarning(
                    "Vatandas sikayeti icin kesin sefer eslesmesi bulunamadi; inceleme atanmadi. ComplaintId: {ComplaintId}",
                    complaint.Id);
                return;
            }

            var garageId = await _context.Trips
                .AsNoTracking()
                .Where(trip => trip.Id == complaint.TripId.Value)
                .Select(trip => (int?)trip.Driver.GarageId)
                .FirstOrDefaultAsync();

            if (!garageId.HasValue)
            {
                _logger.LogWarning(
                    "Vatandas sikayetinin seferine bagli sofor veya garaj bulunamadi; inceleme atanmadi. ComplaintId: {ComplaintId}, TripId: {TripId}",
                    complaint.Id,
                    complaint.TripId.Value);
                return;
            }

            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.GarageId == garageId.Value)
                .OrderBy(inspector => inspector.Investigations.Count(
                    investigation => investigation.ClosedDate == null))
                .ThenBy(inspector => inspector.Id)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (!inspectorId.HasValue)
            {
                _logger.LogWarning(
                    "Vatandas sikayetinin garajinda atanabilir denetimci bulunamadi; inceleme atanmadi. ComplaintId: {ComplaintId}, GarageId: {GarageId}",
                    complaint.Id,
                    garageId.Value);
                return;
            }

            var hasOpenInvestigation = await _context.Investigations
                .AnyAsync(investigation => investigation.ComplaintId == complaint.Id
                    && investigation.ClosedDate == null);

            if (hasOpenInvestigation)
            {
                _logger.LogWarning(
                    "Vatandas sikayeti icin acik inceleme zaten mevcut; yeni inceleme olusturulmadi. ComplaintId: {ComplaintId}",
                    complaint.Id);
                return;
            }

            _context.Investigations.Add(new Investigation
            {
                ComplaintId = complaint.Id,
                InspectorId = inspectorId.Value,
                InvestigationTitle = $"Vatandaş Şikâyeti - {complaint.TrackingCode}",
                InvestigationDescription = "Vatandaş tarafından oluşturulan şikâyet otomatik olarak atanmıştır.",
                InvestigationResult = null,
                ClosedDate = null,
                CreatedDate = createdDate
            });
        }

        private static PublicComplaintOperationStatus Validate(
            CreatePublicComplaintDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DoorNumber))
                return PublicComplaintOperationStatus.DoorNumberRequired;

            if (dto.IncidentDateTime == default)
                return PublicComplaintOperationStatus.IncidentDateTimeRequired;

            if (dto.IncidentDateTime > DateTime.Now.AddMinutes(5))
                return PublicComplaintOperationStatus.IncidentDateTimeInFuture;

            if (string.IsNullOrWhiteSpace(dto.ComplaintDescription))
                return PublicComplaintOperationStatus.DescriptionRequired;

            if (dto.ComplaintDescription.Trim().Length > MaximumDescriptionLength)
                return PublicComplaintOperationStatus.DescriptionTooLong;

            return PublicComplaintOperationStatus.Success;
        }

        private async Task<List<int>> FindMatchingTripIdsAsync(
            int vehicleId,
            int? routeId,
            DateTime incidentDateTime)
        {
            var incidentDate = incidentDateTime.Date;
            var previousDate = incidentDate.AddDays(-1);
            var candidates = await _context.Trips
                .AsNoTracking()
                .Where(item => item.VehicleId == vehicleId
                    && (!routeId.HasValue || item.RouteId == routeId.Value)
                    && item.TripDate >= previousDate
                    && item.TripDate < incidentDate.AddDays(1))
                .Select(item => new
                {
                    item.Id,
                    item.TripDate,
                    item.DepertureTime,
                    item.ArrivalTime
                })
                .ToListAsync();

            var matchingTripIds = candidates
                .Where(item =>
                {
                    var start = item.TripDate.Date + item.DepertureTime;
                    var end = item.TripDate.Date + item.ArrivalTime;

                    if (item.ArrivalTime <= item.DepertureTime)
                    {
                        end = end.AddDays(1);
                    }

                    return start <= incidentDateTime && incidentDateTime < end;
                })
                .Select(item => item.Id)
                .Take(2)
                .ToList();

            return matchingTripIds;
        }

        private async Task<string> GenerateTrackingCodeAsync(int year)
        {
            var prefix = $"SIK-{year}-";
            var existingCodes = await _context.Complaints
                .AsNoTracking()
                .Where(item => item.TrackingCode.StartsWith(prefix))
                .Select(item => item.TrackingCode)
                .ToListAsync();

            var maximumNumber = existingCodes
                .Select(code => code.Trim().ToUpperInvariant())
                .Where(code => code.StartsWith(prefix, StringComparison.Ordinal))
                .Select(code => int.TryParse(code[prefix.Length..], out var number)
                    ? (int?)number
                    : null)
                .Where(number => number.HasValue)
                .Select(number => number!.Value)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{maximumNumber + 1:D6}";
        }
    }
}
