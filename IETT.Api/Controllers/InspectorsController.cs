using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Performances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectorsController : ControllerBase
    {
        private readonly IInspectorService _inspectorService;

        public InspectorsController(IInspectorService inspectorService)
        {
            _inspectorService = inspectorService;
        }

        [HttpGet("me/performances")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyPerformances()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var performances = await _inspectorService
                .GetMyPerformancesAsync(userId);

            return Ok(performances);
        }

        [HttpPost("me/performances")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> CreatePerformance(
            CreateDriverPerformanceDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var performance = await _inspectorService
                .CreatePerformanceAsync(userId, dto);

            if (performance is null)
            {
                return NotFound("Denetimci veya şoför kaydı bulunamadı.");
            }

            return Ok(performance);
        }
    }
}
