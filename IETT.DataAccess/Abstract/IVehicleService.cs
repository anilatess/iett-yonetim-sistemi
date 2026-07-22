using IETT.Entity.Entities;

namespace IETT.Business.Abstract
{
    public interface  IVehicleService
    {
        Task<Vehicle?> GetByIdAsync(int id);
        Task<List<Vehicle>> GetAllAsync();
        Task AddAsync(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(Vehicle vehicle);

    }
}