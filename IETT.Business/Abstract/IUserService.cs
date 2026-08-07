using IETT.Entity.DTOs.Users;

namespace IETT.Business.Abstract
{
    public interface IUserService
    {
        Task<List<UserListDto>> GetAllAsync();
    }
}
