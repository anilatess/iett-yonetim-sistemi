using IETT.Business.Abstract;
using IETT.Entity.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) { _userService = userService; }

        [HttpGet]
        public async Task<ActionResult<List<UserListDto>>> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }
    }
}
