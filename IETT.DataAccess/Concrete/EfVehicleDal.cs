using IETT.DataAccess.Abstract;
using IETT.DataAccess.Context;
using IETT.DataAccess.Repositories;
using IETT.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace IETT.DataAccess.Concrete
{
    public class EfVehicleDal
        : EfEntityRepositoryBase<Vehicle, IETTDbContext>,
          IVehicleDal
    {
        public EfVehicleDal(IETTDbContext context)
            : base(context)
        {
        }

        public async Task<bool> VehicleStatusExistsAsync(
            int vehicleStatusId)
        {
            return await Context.VehicleStatuses
                .AsNoTracking()
                .AnyAsync(status => status.Id == vehicleStatusId);
        }

        public async Task<bool> LicensePlateExistsAsync(
            string licensePlate,
            int? excludedVehicleId = null)
        {
            return await Context.Vehicles
                .AsNoTracking()
                .AnyAsync(vehicle =>
                    vehicle.LicensePlate == licensePlate &&
                    (!excludedVehicleId.HasValue ||
                     vehicle.Id != excludedVehicleId.Value));
        }
    }
}
