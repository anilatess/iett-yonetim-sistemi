using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Trips;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Inspector")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!TryGetRequestIdentity(out var userId, out var role))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var trips = await _tripService.GetAllAsync(userId, role);

            if (trips is null)
            {
                return NotFound(new { message = "Denetimci kaydı bulunamadı." });
            }

            return Ok(trips);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryGetRequestIdentity(out var userId, out var role))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var trip = await _tripService.GetByIdAsync(id, userId, role);

            if (trip is null)
            {
                return NotFound(new
                {
                    message = "Sefer bulunamadı veya bu sefere erişim yetkiniz yok."
                });
            }

            return Ok(trip);
        }

        [HttpPost]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> Create(CreateTripDto dto)
        {
            if (!TryGetRequestIdentity(out var userId, out var role))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _tripService.CreateAsync(dto, userId, role);

            if (result.Status != TripOperationStatus.Success)
            {
                return ToErrorResult(result.Status);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Trip!.Id },
                result.Trip);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> Update(int id, UpdateTripDto dto)
        {
            if (!TryGetRequestIdentity(out var userId, out var role))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _tripService.UpdateAsync(id, dto, userId, role);

            if (result.Status != TripOperationStatus.Success)
            {
                return ToErrorResult(result.Status);
            }

            return NoContent();
        }

        private bool TryGetRequestIdentity(out int userId, out string role)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (!int.TryParse(userIdClaim, out userId))
            {
                return false;
            }

            return role is "Admin" or "Inspector";
        }

        private IActionResult ToErrorResult(TripOperationStatus status)
        {
            return status switch
            {
                TripOperationStatus.InspectorNotFound => NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                }),
                TripOperationStatus.DriverNotFoundOrOutOfScope => NotFound(new
                {
                    message = "Şoför bulunamadı veya bu şoföre erişim yetkiniz yok."
                }),
                TripOperationStatus.TripNotFoundOrOutOfScope => NotFound(new
                {
                    message = "Sefer bulunamadı veya bu sefere erişim yetkiniz yok."
                }),
                TripOperationStatus.VehicleNotFound => NotFound(new
                {
                    message = "Araç bulunamadı."
                }),
                TripOperationStatus.RouteNotFound => NotFound(new
                {
                    message = "Hat bulunamadı."
                }),
                TripOperationStatus.InvalidTripDate => BadRequest(new
                {
                    message = "Sefer tarihi geçersiz."
                }),
                TripOperationStatus.InvalidTripStatus => BadRequest(new
                {
                    message = "Sefer durumu geçersiz."
                }),
                TripOperationStatus.InvalidTimes => BadRequest(new
                {
                    message = "Varış saati kalkış saatinden sonra olmalıdır."
                }),
                _ => BadRequest(new { message = "Sefer bilgileri geçersiz." })
            };
        }
    }
}
