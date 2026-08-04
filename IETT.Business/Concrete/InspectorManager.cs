using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Investigations;
using IETT.Entity.Entities;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class InspectorManager : IInspectorService
    {
        private readonly IETTDbContext _context;

        public InspectorManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<DriverPerformanceDto?> CreatePerformanceAsync(
            int userId,
            CreateDriverPerformanceDto dto)
        {
            var inspector = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => new
                {
                    inspector.Id,
                    inspector.GarageId,
                    inspector.User.FirstName,
                    inspector.User.LastName
                })
                .FirstOrDefaultAsync();

            if (inspector is null)
            {
                return null;
            }

            var driverExists = await _context.Drivers
                .AsNoTracking()
                .AnyAsync(driver =>
                    driver.Id == dto.DriverId
                    && driver.GarageId == inspector.GarageId);

            if (!driverExists)
            {
                return null;
            }

            var performance = new DriverPerformance
            {
                DriverId = dto.DriverId,
                InspectorId = inspector.Id,
                Score = dto.Score,
                PerformanceComment = dto.PerformanceComment ?? string.Empty,
                EvaluationDate = DateTime.Now
            };

            _context.DriverPerformances.Add(performance);
            await _context.SaveChangesAsync();

            return new DriverPerformanceDto
            {
                Id = performance.Id,
                Score = performance.Score,
                PerformanceComment = performance.PerformanceComment ?? string.Empty,
                EvaluationDate = performance.EvaluationDate,
                InspectorFullName = inspector.FirstName + " " + inspector.LastName
            };
        }

        public async Task<List<InspectorPerformanceHistoryDto>> GetMyPerformancesAsync(
            int userId)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return new List<InspectorPerformanceHistoryDto>();
            }

            return await _context.DriverPerformances
                .AsNoTracking()
                .Where(performance => performance.InspectorId == inspectorId.Value)
                .OrderByDescending(performance => performance.EvaluationDate)
                .Select(performance => new InspectorPerformanceHistoryDto
                {
                    Id = performance.Id,
                    DriverId = performance.DriverId,
                    DriverFullName = performance.Driver.User.FirstName
                        + " " + performance.Driver.User.LastName,
                    PersonnelNumber = performance.Driver.PersonnelNumber,
                    Score = performance.Score,
                    PerformanceComment = performance.PerformanceComment ?? string.Empty,
                    EvaluationDate = performance.EvaluationDate
                })
                .ToListAsync();
        }

        public async Task<List<InspectorInvestigationDto>?> GetMyInvestigationsAsync(
            int userId)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return null;
            }

            return await _context.Investigations
                .AsNoTracking()
                .Where(investigation => investigation.InspectorId == inspectorId.Value)
                .OrderBy(investigation => investigation.ClosedDate.HasValue)
                .ThenByDescending(investigation => investigation.CreatedDate)
                .Select(investigation => new InspectorInvestigationDto
                {
                    Id = investigation.Id,
                    ComplaintId = investigation.ComplaintId,
                    TrackingCode = investigation.Complaint.TrackingCode,
                    ComplaintTypeName = investigation.Complaint.ComplaintType.ComplaintTypeName,
                    ComplaintDescription = investigation.Complaint.ComplaintDescription,
                    ComplaintCreatedDate = investigation.Complaint.CreatedDate,
                    InvestigationTitle = investigation.InvestigationTitle,
                    InvestigationDescription = investigation.InvestigationDescription ?? string.Empty,
                    InvestigationResult = investigation.InvestigationResult ?? string.Empty,
                    InvestigationCreatedDate = investigation.CreatedDate,
                    ClosedDate = investigation.ClosedDate,
                    Status = investigation.ClosedDate.HasValue ? "Tamamlandı" : "Devam Ediyor",
                    DriverId = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.DriverId,
                    DriverFullName = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.Driver.User.FirstName
                            + " " + investigation.Complaint.Trip.Driver.User.LastName,
                    PersonnelNumber = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.Driver.PersonnelNumber,
                    VehicleId = investigation.Complaint.VehicleId,
                    VehicleDoorNumber = investigation.Complaint.Vehicle.DoorNumber,
                    RouteId = investigation.Complaint.RouteId,
                    RouteCode = investigation.Complaint.BusRoute.RouteCode,
                    RouteName = investigation.Complaint.BusRoute.RouteName,
                    StopId = investigation.Complaint.StopId,
                    StopCode = investigation.Complaint.BusStop.StopCode,
                    StopName = investigation.Complaint.BusStop.StopName,
                    TripId = investigation.Complaint.TripId,
                    TripDate = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.TripDate,
                    DepertureTime = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.DepertureTime,
                    ArrivalTime = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.ArrivalTime
                })
                .ToListAsync();
        }

        public async Task<InvestigationCompletionStatus> CompleteInvestigationAsync(
            int userId,
            int investigationId,
            CompleteInvestigationDto dto)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return InvestigationCompletionStatus.NotFound;
            }

            var investigation = await _context.Investigations
                .Include(item => item.Complaint)
                .FirstOrDefaultAsync(item =>
                    item.Id == investigationId
                    && item.InspectorId == inspectorId.Value);

            if (investigation is null)
            {
                return InvestigationCompletionStatus.NotFound;
            }

            if (investigation.ClosedDate.HasValue)
            {
                return InvestigationCompletionStatus.AlreadyCompleted;
            }

            investigation.InvestigationResult = dto.InvestigationResult.Trim();
            investigation.ClosedDate = DateTime.Now;

            var hasAnotherOpenInvestigation = await _context.Investigations
                .AsNoTracking()
                .AnyAsync(item =>
                    item.ComplaintId == investigation.ComplaintId
                    && item.Id != investigation.Id
                    && !item.ClosedDate.HasValue);

            if (!hasAnotherOpenInvestigation)
            {
                investigation.Complaint.ComplaintStatus = ComplaintStatusEnum.Resolved;
            }

            await _context.SaveChangesAsync();
            return InvestigationCompletionStatus.Completed;
        }
    }
}
