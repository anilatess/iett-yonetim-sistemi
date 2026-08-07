using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Users;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IETTDbContext _context;

        public UserManager(IETTDbContext context) { _context = context; }

        public Task<List<UserListDto>> GetAllAsync() => _context.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(user => new UserListDto
            {
                Id = user.Id,
                FullName = (user.FirstName + " " + user.LastName).Trim(),
                UserName = user.UserName,
                RoleId = user.RoleId,
                RoleName = user.Role.RoleName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedDate = user.CreatedDate
            })
            .ToListAsync();
    }
}
