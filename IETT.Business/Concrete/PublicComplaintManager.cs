using System.Data;
using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Complaints;
using IETT.Entity.Entities;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class PublicComplaintManager : IPublicComplaintService
    {
        public const int MaximumDescriptionLength = 2000;

        private readonly IETTDbContext _context;

        public PublicComplaintManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<PublicComplaintOperationResult> CreateAsync(
            CreatePublicComplaintDto dto)
        {
            var validation = await ValidateAsync(dto);

            if (validation != PublicComplaintOperationStatus.Success)
            {
                return PublicComplaintOperationResult.Failure(validation);
            }

            var now = DateTime.Now;
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var trackingCode = await GenerateTrackingCodeAsync(now.Year);
            var complaint = new Complaint
            {
                TrackingCode = trackingCode,
                ComplaintTypeId = dto.ComplaintTypeId,
                RouteId = dto.RouteId,
                VehicleId = dto.VehicleId,
                StopId = dto.StopId,
                TripId = dto.TripId,
                ComplaintDate = dto.ComplaintDate!.Value,
                ComplaintTime = dto.ComplaintTime!.Value,
                ComplaintDescription = dto.ComplaintDescription.Trim(),
                ComplaintStatus = ComplaintStatusEnum.Pending,
                CreatedDate = now
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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

        private async Task<PublicComplaintOperationStatus> ValidateAsync(
            CreatePublicComplaintDto dto)
        {
            if (!dto.ComplaintDate.HasValue)
                return PublicComplaintOperationStatus.ComplaintDateRequired;

            if (!dto.ComplaintTime.HasValue)
                return PublicComplaintOperationStatus.ComplaintTimeRequired;

            if (string.IsNullOrWhiteSpace(dto.ComplaintDescription))
                return PublicComplaintOperationStatus.DescriptionRequired;

            if (dto.ComplaintDescription.Trim().Length > MaximumDescriptionLength)
                return PublicComplaintOperationStatus.DescriptionTooLong;

            if (!await _context.ComplaintTypes.AsNoTracking()
                    .AnyAsync(item => item.Id == dto.ComplaintTypeId))
                return PublicComplaintOperationStatus.ComplaintTypeNotFound;

            if (!await _context.BusRoutes.AsNoTracking()
                    .AnyAsync(item => item.Id == dto.RouteId))
                return PublicComplaintOperationStatus.RouteNotFound;

            if (!await _context.Vehicles.AsNoTracking()
                    .AnyAsync(item => item.Id == dto.VehicleId))
                return PublicComplaintOperationStatus.VehicleNotFound;

            if (!await _context.BusStops.AsNoTracking()
                    .AnyAsync(item => item.Id == dto.StopId))
                return PublicComplaintOperationStatus.StopNotFound;

            if (!dto.TripId.HasValue)
                return PublicComplaintOperationStatus.Success;

            var trip = await _context.Trips.AsNoTracking()
                .Where(item => item.Id == dto.TripId.Value)
                .Select(item => new { item.RouteId, item.VehicleId })
                .FirstOrDefaultAsync();

            if (trip is null)
                return PublicComplaintOperationStatus.TripNotFound;

            if (trip.RouteId != dto.RouteId)
                return PublicComplaintOperationStatus.TripRouteMismatch;

            if (trip.VehicleId != dto.VehicleId)
                return PublicComplaintOperationStatus.TripVehicleMismatch;

            return PublicComplaintOperationStatus.Success;
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
