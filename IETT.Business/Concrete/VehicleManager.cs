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

        public async Task UpdateAsync(UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == dto.Id);

            if (vehicle == null)
            {
                throw new KeyNotFoundException("Güncellenecek araç bulunamadı.");
            }

            vehicle.DoorNumber = dto.DoorNumber;
            vehicle.VehicleStatusId = dto.VehicleStatusId;

            _vehicleDal.Update(vehicle);
        }

        public async Task<bool> UpdateStatusAsync(
            int vehicleId,
            int vehicleStatusId)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == vehicleId);

            if (vehicle == null)
            {
                return false;
            }

            var statusExists = await _vehicleDal
                .VehicleStatusExistsAsync(vehicleStatusId);

            if (!statusExists)
            {
                throw new ArgumentException(
                    "Gönderilen araç durumu bulunamadı.");
            }

            vehicle.VehicleStatusId = vehicleStatusId;
            _vehicleDal.Update(vehicle);

            return true;
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
