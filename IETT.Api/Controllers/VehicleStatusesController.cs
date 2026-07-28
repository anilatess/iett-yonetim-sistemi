using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Vehicle;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleStatusesController : ControllerBase
    {
        private readonly IETTDbContext _context;

        public VehicleStatusesController(IETTDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<VehicleStatusDto>>> GetAll()
        {
            var statuses = await _context.VehicleStatuses
                .AsNoTracking()
                .Select(status => new VehicleStatusDto
                {
                    Id = status.Id,
                    Name = status.Name
                })
                .ToListAsync();

            return Ok(statuses);
        }
    }
}