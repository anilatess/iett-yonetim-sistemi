using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Investigations;
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

        [HttpGet("me/investigations")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyInvestigations()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var investigations = await _inspectorService
                .GetMyInvestigationsAsync(userId);

            if (investigations is null)
            {
                return NotFound("Denetimci kaydı bulunamadı.");
            }

            return Ok(investigations);
        }

        [HttpPut("me/investigations/{id:int}/complete")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> CompleteInvestigation(
            int id,
            CompleteInvestigationDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var completionStatus = await _inspectorService
                .CompleteInvestigationAsync(userId, id, dto);

            return completionStatus switch
            {
                InvestigationCompletionStatus.Completed => NoContent(),
                InvestigationCompletionStatus.AlreadyCompleted => Conflict(new
                {
                    message = "Bu inceleme görevi daha önce sonuçlandırılmış."
                }),
                _ => NotFound(new
                {
                    message = "İnceleme görevi bulunamadı."
                })
            };
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
