using IETT.Business.Abstract;
using IETT.Entity.DTOs.Investigations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InvestigationsController : ControllerBase
    {
        private readonly IInvestigationService _investigationService;

        public InvestigationsController(
            IInvestigationService investigationService)
        {
            _investigationService = investigationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminInvestigationDto>>> GetAll()
        {
            return Ok(await _investigationService.GetAllForAdminAsync());
        }
    }
}
