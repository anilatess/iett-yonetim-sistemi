using IETT.Core.DataAccess;
using IETT.DataAccess.Abstract;
using IETT.DataAccess.Context;
using IETT.DataAccess.Repositories;
using IETT.Entity.Entities;

namespace IETT.DataAccess.Concrete
{
    public class EfBusRouteDal
        : EfEntityRepositoryBase<BusRoute, IETTDbContext>, IBusRouteDal
    {
        public EfBusRouteDal(IETTDbContext context)
            : base(context)
        {
        }
    }
}