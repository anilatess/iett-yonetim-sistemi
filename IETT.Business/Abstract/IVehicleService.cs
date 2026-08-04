using IETT.Entity.DTOs.Vehicles;

namespace IETT.Business.Abstract
{
    public interface IVehicleService
    {
        Task<List<VehicleDto>> GetAllAsync();

        Task<VehicleDto?> GetByIdAsync(int id);

        Task<VehicleDto> AddAsync(CreateVehicleDto dto);

        Task UpdateAsync(UpdateVehicleDto dto);

        Task<bool> UpdateStatusAsync(
            int vehicleId,
            int vehicleStatusId);

        Task<bool> DeleteAsync(int id);
    }
}
