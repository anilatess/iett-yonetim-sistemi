using IETT.Business.Abstract;
using IETT.DataAccess.Abstract;
using IETT.Entity.DTOs.Vehicles;
using IETT.Entity.Entities;

namespace IETT.Business.Concrete
{
    public class VehicleManager : IVehicleService
    {
        private readonly IVehicleDal _vehicleDal;

        public VehicleManager(IVehicleDal vehicleDal)
        {
            _vehicleDal = vehicleDal;
        }

        public async Task<List<VehicleDto>> GetAllAsync()
        {
            var vehicles = await _vehicleDal.GetListAsync();

            return vehicles.Select(vehicle => new VehicleDto
            {
                Id = vehicle.Id,
                DoorNumber = vehicle.DoorNumber,
                VehicleStatusId = vehicle.VehicleStatusId
            }).ToList();
        }

        public async Task<VehicleDto?> GetByIdAsync(int id)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == id);

            if (vehicle == null)
            {
                return null;
            }

            return new VehicleDto
            {
                Id = vehicle.Id,
                DoorNumber = vehicle.DoorNumber,
                VehicleStatusId = vehicle.VehicleStatusId
            };
        }

        public async Task<VehicleDto> AddAsync(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                DoorNumber = dto.DoorNumber,
                VehicleStatusId = dto.VehicleStatusId
            };

            await _vehicleDal.AddAsync(vehicle);

            return new VehicleDto
            {
                Id = vehicle.Id,
                DoorNumber = vehicle.DoorNumber,
                VehicleStatusId = vehicle.VehicleStatusId
            };
        }

        public Task UpdateAsync(UpdateVehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                Id = dto.Id,
                DoorNumber = dto.DoorNumber,
                VehicleStatusId = dto.VehicleStatusId
            };

            _vehicleDal.Update(vehicle);

            return Task.CompletedTask;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == id);

            if (vehicle == null)
            {
                return false;
            }

            _vehicleDal.Delete(vehicle);

            return true;
        }
    }
}