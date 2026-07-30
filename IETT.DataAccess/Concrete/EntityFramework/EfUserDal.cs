using IETT.DataAccess.Abstract;
using IETT.DataAccess.Context;
using IETT.DataAccess.Repositories;
using IETT.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace IETT.DataAccess.Concrete.EntityFramework
{
    public class EfUserDal
        : EfEntityRepositoryBase<User, IETTDbContext>, IUserDal
    {
        private readonly IETTDbContext _context;

        public EfUserDal(IETTDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetUserWithRoleAsync(string userName)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserName == userName);
        }
    }
}