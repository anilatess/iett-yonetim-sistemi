using IETT.Core.DataAccess;
using IETT.Entity.Entities;

namespace IETT.DataAccess.Abstract
{
    public interface IUserDal : IEntityRepository<User>
    {
        Task<User?> GetUserWithRoleAsync(string userName);
    }
}