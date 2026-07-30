using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Kullanıcı adı veya şifre hatalı."
                });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userName =
                User.FindFirstValue(ClaimTypes.Name);

            var role =
                User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                userId,
                userName,
                role
            });
        }
    }
}