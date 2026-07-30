using IETT.Business.Abstract;
using IETT.DataAccess.Abstract;
using IETT.Entity.DTOs.Auth;
using IETT.Entity.Entities;
using Microsoft.AspNetCore.Identity;

namespace IETT.Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private readonly IUserDal _userDal;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthManager(
            IUserDal userDal,
            IPasswordHasher<User> passwordHasher,
            ITokenService tokenService)
        {
            _userDal = userDal;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userDal
                .GetUserWithRoleAsync(loginDto.UserName);

            if (user == null)
            {
                return null;
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginDto.Password
            );

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var token = _tokenService.CreateToken(user);

            return new LoginResponseDto
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                UserName = user.UserName,
                Role = user.Role.RoleName,
                Token = token
            };
        }
    }
}