using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Investigations;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class InvestigationManager : IInvestigationService
    {
        private readonly IETTDbContext _context;

        public InvestigationManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdminInvestigationDto>> GetAllForAdminAsync()
        {
            return await _context.Investigations
                .AsNoTracking()
                .OrderBy(investigation => investigation.ClosedDate.HasValue)
                .ThenByDescending(investigation =>
                    investigation.ClosedDate ?? investigation.CreatedDate)
                .ThenByDescending(investigation => investigation.Id)
                .Select(investigation => new AdminInvestigationDto
                {
                    InvestigationId = investigation.Id,
                    InspectorId = investigation.InspectorId,
                    InspectorFullName = investigation.Inspector.User.FirstName
                        + " " + investigation.Inspector.User.LastName,
                    ComplaintId = investigation.ComplaintId,
                    TrackingCode = investigation.Complaint.TrackingCode,
                    ComplaintTypeName = investigation.Complaint.ComplaintType.ComplaintTypeName,
                    ComplaintDescription = investigation.Complaint.ComplaintDescription,
                    DriverId = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.DriverId,
                    DriverFullName = investigation.Complaint.Trip == null
                        ? "Belirtilmedi"
                        : investigation.Complaint.Trip.Driver.User.FirstName
                            + " " + investigation.Complaint.Trip.Driver.User.LastName,
                    DriverPersonnelNumber = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.Driver.PersonnelNumber,
                    VehicleId = investigation.Complaint.VehicleId,
                    VehicleDoorNumber = investigation.Complaint.Vehicle.DoorNumber,
                    RouteId = investigation.Complaint.RouteId,
                    RouteCode = investigation.Complaint.BusRoute.RouteCode,
                    RouteName = investigation.Complaint.BusRoute.RouteName,
                    StopId = investigation.Complaint.StopId,
                    StopCode = investigation.Complaint.BusStop == null
                        ? null
                        : investigation.Complaint.BusStop.StopCode,
                    StopName = investigation.Complaint.BusStop == null
                        ? "Belirtilmedi"
                        : investigation.Complaint.BusStop.StopName,
                    TripId = investigation.Complaint.TripId,
                    TripDate = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.TripDate,
                    DepertureTime = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.DepertureTime,
                    ArrivalTime = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.ArrivalTime,
                    CreatedDate = investigation.CreatedDate,
                    ClosedDate = investigation.ClosedDate,
                    InvestigationTitle = investigation.InvestigationTitle,
                    InvestigationDescription = investigation.InvestigationDescription
                        ?? string.Empty,
                    InvestigationResult = investigation.InvestigationResult,
                    Status = investigation.ClosedDate == null
                        ? "Devam Ediyor"
                        : "Tamamlandı",
                    Decision = investigation.ClosedDate == null
                        ? "Bekliyor"
                        : investigation.Complaint.ComplaintStatus
                            == ComplaintStatusEnum.ForwardedToDriver
                            ? "Şoföre İletildi"
                            : investigation.Complaint.ComplaintStatus
                                == ComplaintStatusEnum.Rejected
                                ? "Reddedildi"
                                : investigation.Complaint.ComplaintStatus
                                    == ComplaintStatusEnum.Resolved
                                    ? "Çözüldü"
                                    : investigation.Complaint.ComplaintStatus
                                        == ComplaintStatusEnum.UnderReview
                                        ? "İnceleniyor"
                                        : "Bekliyor"
                })
                .ToListAsync();
        }
    }
}
