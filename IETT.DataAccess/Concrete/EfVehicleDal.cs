using IETT.DataAccess.Abstract;
using IETT.DataAccess.Context;
using IETT.DataAccess.Repositories;
using IETT.Entity.Entities;

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
    }
}