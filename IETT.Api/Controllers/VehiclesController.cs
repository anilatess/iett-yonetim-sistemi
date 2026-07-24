using IETT.Entity.DTOs.Vehicles;    
using IETT.Business.Abstract;
using IETT.Entity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleService.GetAllAsync();

            var result = vehicles.Select(vehicle => new
            {
                vehicle.Id,
                vehicle.DoorNumber,
                vehicle.VehicleStatusId
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
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
        public async Task<IActionResult> Add(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                DoorNumber = dto.DoorNumber,
                VehicleStatusId = dto.VehicleStatusId
            };

            try
            {
                await _vehicleService.AddAsync(vehicle);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = vehicle.Id },
                    new
                    {
                        vehicle.Id,
                        vehicle.DoorNumber,
                        vehicle.VehicleStatusId
                    });
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
        public IActionResult Update(UpdateVehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                Id = dto.Id,
                DoorNumber = dto.DoorNumber,
                VehicleStatusId = dto.VehicleStatusId
            };

            try
            {
                _vehicleService.Update(vehicle);
                return NoContent();
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
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);

            if (vehicle == null)
            {
                return NotFound(new
                {
                    message = "Silinecek araç bulunamadı."
                });
            }

            _vehicleService.Delete(vehicle);
            return NoContent();
        }
    }
}