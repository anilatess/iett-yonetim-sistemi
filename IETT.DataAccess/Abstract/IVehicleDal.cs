using IETT.Core.DataAccess;
using IETT.Entity.Entities;

namespace IETT.DataAccess.Abstract
{
    public interface IVehicleDal : IEntityRepository<Vehicle>
    {
        Task<bool> VehicleStatusExistsAsync(int vehicleStatusId);
        Task<bool> LicensePlateExistsAsync(
            string licensePlate,
            int? excludedVehicleId = null);
    }
}
