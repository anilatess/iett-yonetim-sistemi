using IETT.Business.Abstract;
using IETT.DataAccess.Abstract;
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

        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            return await _vehicleDal.GetAsync(x => x.Id == id);
        }

        public async Task<List<Vehicle>> GetAllAsync()
        {
            return await _vehicleDal.GetListAsync();
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            await _vehicleDal.AddAsync(vehicle);
        }

        public void Update(Vehicle vehicle)
        {
            _vehicleDal.Update(vehicle);
        }

        public void Delete(Vehicle vehicle)
        {
            _vehicleDal.Delete(vehicle);
        }
    }
}