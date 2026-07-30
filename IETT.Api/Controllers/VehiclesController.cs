using IETT.Business.Abstract;
using IETT.Entity.DTOs.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleService.GetAllAsync();

            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);

            if (vehicle == null)
            {
                return NotFound(new
                {
                    message = "Araç bulunamadı."
                });
            }

            return Ok(vehicle);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(CreateVehicleDto dto)
        {
            try
            {
                var createdVehicle = await _vehicleService.AddAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdVehicle.Id },
                    createdVehicle);
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    message = "Geçersiz araç durumu. Gönderilen VehicleStatusId veritabanında bulunamadı."
                });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(UpdateVehicleDto dto)
        {
            try
            {
                await _vehicleService.UpdateAsync(dto);

                return NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new
                {
                    message = exception.Message
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    message = "Araç güncellenemedi. VehicleStatusId geçersiz olabilir."
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _vehicleService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Silinecek araç bulunamadı."
                });
            }

            return NoContent();
        }
    }
}
