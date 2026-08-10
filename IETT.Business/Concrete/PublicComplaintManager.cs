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

        public async Task<PublicComplaintOperationResult> CreateAsync(
            CreatePublicComplaintDto dto)
        {
            var validation = Validate(dto);

            if (validation != PublicComplaintOperationStatus.Success)
            {
                return PublicComplaintOperationResult.Failure(validation);
            }

            var doorNumber = dto.DoorNumber.Trim();
            var routeCode = dto.RouteCode.Trim();

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

            var routeIds = await _context.BusRoutes
                .AsNoTracking()
                .Where(item => item.RouteCode.ToUpper() == routeCode.ToUpper())
                .OrderBy(item => item.Id)
                .Select(item => item.Id)
                .Take(2)
                .ToListAsync();

            if (routeIds.Count == 0)
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
            var routeId = routeIds[0];
            var tripId = await FindExactTripIdAsync(
                vehicleId,
                routeId,
                dto.IncidentDateTime);

            var now = DateTime.Now;
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var trackingCode = await GenerateTrackingCodeAsync(now.Year);
            var complaint = new Complaint
            {
                TrackingCode = trackingCode,
                ComplaintTypeId = dto.ComplaintTypeId,
                RouteId = routeId,
                VehicleId = vehicleId,
                StopId = null,
                TripId = tripId,
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
                await TryCreateInvestigationAsync(complaint, now);
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

            if (string.IsNullOrWhiteSpace(dto.RouteCode))
                return PublicComplaintOperationStatus.RouteCodeRequired;

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

        private async Task<int?> FindExactTripIdAsync(
            int vehicleId,
            int routeId,
            DateTime incidentDateTime)
        {
            var incidentDate = incidentDateTime.Date;
            var previousDate = incidentDate.AddDays(-1);
            var candidates = await _context.Trips
                .AsNoTracking()
                .Where(item => item.VehicleId == vehicleId
                    && item.RouteId == routeId
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

            return matchingTripIds.Count == 1 ? matchingTripIds[0] : null;
        }

        private async Task<string> GenerateTrackingCodeAsync(int year)
        {
            var prefix = $"SIK-{year}-";
            var lastCode = await _context.Complaints
                .Where(item => item.TrackingCode.StartsWith(prefix))
                .OrderByDescending(item => item.TrackingCode)
                .Select(item => item.TrackingCode)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastCode is not null &&
                int.TryParse(lastCode[prefix.Length..], out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D6}";
        }
    }
}
