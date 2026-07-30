using IETT.Entity.DTOs.Auth;

namespace IETT.Business.Abstract
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    }
}