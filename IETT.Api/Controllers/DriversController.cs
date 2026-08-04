using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Drivers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriversController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<ActionResult<List<DriverListDto>>> GetAll()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role is not ("Admin" or "Inspector"))
            {
                return Forbid();
            }

            var drivers = await _driverService
                .GetAllAsync(userId, role);

            if (drivers is null)
            {
                return NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                });
            }

            return Ok(drivers);
        }

        [HttpGet("me/trips")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyTrips()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var trips =
                await _driverService.GetMyTripsAsync(userId);

            return Ok(trips);
        }

        [HttpGet("me/complaints")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyComplaints()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var complaints =
                await _driverService.GetMyComplaintsAsync(userId);

            if (complaints is null)
            {
                return NotFound(new
                {
                    message = "Şoför kaydı bulunamadı."
                });
            }

            return Ok(complaints);
        }

        [HttpGet("me/certificates")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyCertificates()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var certificates =
                await _driverService.GetMyCertificatesAsync(userId);

            return Ok(certificates);
        }

        [HttpGet("{driverId:int}/certificates")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetCertificates(int driverId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role is not ("Admin" or "Inspector"))
            {
                return Forbid();
            }

            var certificates = await _driverService.GetCertificatesAsync(
                userId,
                role,
                driverId);

            if (certificates is null)
            {
                return NotFound(new
                {
                    message = "Şoför bulunamadı veya bu şoföre erişim yetkiniz yok."
                });
            }

            return Ok(certificates);
        }

        [HttpGet("me/performances")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyPerformances()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var performances =
                await _driverService.GetMyPerformancesAsync(userId);

            return Ok(performances);
        }

    }
}
