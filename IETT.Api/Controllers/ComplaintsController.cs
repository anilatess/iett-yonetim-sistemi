using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Complaints;
using IETT.Entity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ComplaintsController : ControllerBase
    {
        private readonly IETTDbContext _context;

        public ComplaintsController(IETTDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminComplaintDto>>> GetAll()
        {
            var complaints = await ProjectComplaints()
                .OrderByDescending(complaint => complaint.CreatedDate)
                .ToListAsync();

            return Ok(complaints);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminComplaintDto>> GetById(int id)
        {
            var complaint = await ProjectComplaints()
                .SingleOrDefaultAsync(item => item.Id == id);

            if (complaint is null)
            {
                return NotFound(new { message = "Şikâyet bulunamadı." });
            }

            return Ok(complaint);
        }

        private IQueryable<AdminComplaintDto> ProjectComplaints()
        {
            return _context.Complaints
                .AsNoTracking()
                .Select(complaint => new AdminComplaintDto
                {
                    Id = complaint.Id,
                    TrackingCode = complaint.TrackingCode,
                    ComplaintTypeName = complaint.ComplaintType.ComplaintTypeName,
                    ComplaintDescription = complaint.ComplaintDescription,
                    ComplaintDate = complaint.ComplaintDate,
                    ComplaintTime = complaint.ComplaintTime,
                    CreatedDate = complaint.CreatedDate,
                    ComplaintStatus = complaint.ComplaintStatus,
                    ComplaintStatusName = complaint.ComplaintStatus == ComplaintStatusEnum.Pending
                        ? "Beklemede"
                        : complaint.ComplaintStatus == ComplaintStatusEnum.UnderReview
                            ? "İnceleniyor"
                            : complaint.ComplaintStatus == ComplaintStatusEnum.Resolved
                                ? "Çözüldü"
                                : complaint.ComplaintStatus == ComplaintStatusEnum.ForwardedToDriver
                                    ? "Şoföre İletildi"
                                    : "Reddedildi",
                    RouteId = complaint.RouteId,
                    RouteCode = complaint.BusRoute == null ? null : complaint.BusRoute.RouteCode,
                    RouteName = complaint.BusRoute == null ? null : complaint.BusRoute.RouteName,
                    VehicleId = complaint.VehicleId,
                    VehicleDoorNumber = complaint.Vehicle.DoorNumber,
                    StopId = complaint.StopId,
                    StopCode = complaint.BusStop != null
                        ? complaint.BusStop.StopCode
                        : "Belirtilmedi",
                    StopName = complaint.BusStop != null
                        ? complaint.BusStop.StopName
                        : "Belirtilmedi",
                    TripId = complaint.TripId,
                    TripDate = complaint.Trip != null ? complaint.Trip.TripDate : null,
                    DepertureTime = complaint.Trip != null ? complaint.Trip.DepertureTime : null,
                    ArrivalTime = complaint.Trip != null ? complaint.Trip.ArrivalTime : null,
                    DriverId = complaint.Trip != null ? complaint.Trip.DriverId : null,
                    DriverFullName = complaint.Trip != null
                        ? complaint.Trip.Driver.User.FirstName + " " + complaint.Trip.Driver.User.LastName
                        : null,
                    DriverPersonnelNumber = complaint.Trip != null ? complaint.Trip.Driver.PersonnelNumber : null,
                    InvestigationId = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => (int?)item.Id).FirstOrDefault(),
                    InvestigationTitle = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => item.InvestigationTitle).FirstOrDefault(),
                    InvestigationDescription = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => item.InvestigationDescription).FirstOrDefault(),
                    InvestigationResult = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => item.InvestigationResult).FirstOrDefault(),
                    InvestigationCreatedDate = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => (DateTime?)item.CreatedDate).FirstOrDefault(),
                    InvestigationClosedDate = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => item.ClosedDate).FirstOrDefault(),
                    InspectorId = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => (int?)item.InspectorId).FirstOrDefault(),
                    InspectorFullName = complaint.Investigations
                        .OrderByDescending(item => item.CreatedDate)
                        .Select(item => item.Inspector.User.FirstName + " " + item.Inspector.User.LastName)
                        .FirstOrDefault()
                });
        }
    }
}
