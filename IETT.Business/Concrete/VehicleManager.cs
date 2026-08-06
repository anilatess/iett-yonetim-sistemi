using IETT.Business.Abstract;
using IETT.Business.Exceptions;
using IETT.DataAccess.Abstract;
using IETT.Entity.DTOs.Vehicles;
using IETT.Entity.Entities;

namespace IETT.Business.Concrete
{
    public class VehicleManager : IVehicleService
    {
        private readonly IVehicleDal _vehicleDal;

        public VehicleManager(IVehicleDal vehicleDal) => _vehicleDal = vehicleDal;

        public async Task<List<VehicleDto>> GetAllAsync() =>
            (await _vehicleDal.GetListAsync()).Select(Map).ToList();

        public async Task<VehicleDto?> GetByIdAsync(int id)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == id);
            return vehicle == null ? null : Map(vehicle);
        }

        public async Task<VehicleDto> AddAsync(CreateVehicleDto dto)
        {
            var licensePlate = NormalizeLicensePlate(dto.LicensePlate);
            ValidateTextFields(dto.DoorNumber, licensePlate, dto.Model);

            if (await _vehicleDal.LicensePlateExistsAsync(licensePlate))
                throw new VehicleLicensePlateConflictException();

            await EnsureStatusExistsAsync(dto.VehicleStatusId);

            var vehicle = new Vehicle
            {
                DoorNumber = dto.DoorNumber.Trim(),
                LicensePlate = licensePlate,
                Capacity = dto.Capacity,
                Model = dto.Model.Trim(),
                ProductionYear = dto.ProductionYear,
                VehicleStatusId = dto.VehicleStatusId
            };

            await _vehicleDal.AddAsync(vehicle);
            return Map(vehicle);
        }

        public async Task UpdateAsync(UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == dto.Id);
            if (vehicle == null)
                throw new KeyNotFoundException("Güncellenecek araç bulunamadı.");

            var licensePlate = NormalizeLicensePlate(dto.LicensePlate);
            ValidateTextFields(dto.DoorNumber, licensePlate, dto.Model);

            if (await _vehicleDal.LicensePlateExistsAsync(licensePlate, vehicle.Id))
                throw new VehicleLicensePlateConflictException();

            await EnsureStatusExistsAsync(dto.VehicleStatusId);

            vehicle.DoorNumber = dto.DoorNumber.Trim();
            vehicle.LicensePlate = licensePlate;
            vehicle.Capacity = dto.Capacity;
            vehicle.Model = dto.Model.Trim();
            vehicle.ProductionYear = dto.ProductionYear;
            vehicle.VehicleStatusId = dto.VehicleStatusId;
            _vehicleDal.Update(vehicle);
        }

        public async Task<bool> UpdateStatusAsync(int vehicleId, int vehicleStatusId)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == vehicleId);
            if (vehicle == null) return false;

            await EnsureStatusExistsAsync(vehicleStatusId);
            vehicle.VehicleStatusId = vehicleStatusId;
            _vehicleDal.Update(vehicle);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vehicle = await _vehicleDal.GetAsync(x => x.Id == id);
            if (vehicle == null) return false;
            _vehicleDal.Delete(vehicle);
            return true;
        }

        private async Task EnsureStatusExistsAsync(int vehicleStatusId)
        {
            if (!await _vehicleDal.VehicleStatusExistsAsync(vehicleStatusId))
                throw new ArgumentException("Gönderilen araç durumu bulunamadı.");
        }

        private static VehicleDto Map(Vehicle vehicle) => new()
        {
            Id = vehicle.Id,
            DoorNumber = vehicle.DoorNumber,
            LicensePlate = vehicle.LicensePlate,
            Capacity = vehicle.Capacity,
            Model = vehicle.Model,
            ProductionYear = vehicle.ProductionYear,
            VehicleStatusId = vehicle.VehicleStatusId
        };

        private static string NormalizeLicensePlate(string licensePlate) =>
            licensePlate.Trim().ToUpperInvariant();

        private static void ValidateTextFields(string doorNumber, string licensePlate, string model)
        {
            if (string.IsNullOrWhiteSpace(doorNumber))
                throw new ArgumentException("Kapı numarası zorunludur.");
            if (string.IsNullOrWhiteSpace(licensePlate))
                throw new ArgumentException("Plaka zorunludur.");
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model zorunludur.");
        }
    }
}
