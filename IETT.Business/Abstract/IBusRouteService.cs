using IETT.Entity.DTOs.BusRouteDtos;

namespace IETT.Business.Abstract
{
    public interface IBusRouteService
    {
        Task<List<BusRouteDto>> GetAllAsync();
        Task<BusRouteDto?> GetByIdAsync(int id);
        Task<BusRouteDto> AddAsync(CreateBusRouteDto dto);
        Task UpdateAsync(UpdateBusRouteDto dto);
        Task<bool> DeleteAsync(int id);
    }
}