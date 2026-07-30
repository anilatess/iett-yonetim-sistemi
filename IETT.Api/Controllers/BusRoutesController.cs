using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.BusRouteDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusRoutesController : ControllerBase
    {
        private readonly IBusRouteService _busRouteService;
        private readonly IETTDbContext _context;

        public BusRoutesController(
            IBusRouteService busRouteService,
            IETTDbContext context)
        {
            _busRouteService = busRouteService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetAll()
        {
            var routes = await _busRouteService.GetAllAsync();

            return Ok(routes);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetById(int id)
        {
            var route = await _busRouteService.GetByIdAsync(id);

            if (route == null)
            {
                return NotFound("Hat bulunamadı.");
            }

            return Ok(route);
        }

        [HttpGet("{routeId:int}/stops")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetStopsByRouteId(
            int routeId)
        {
            var route = await _context.BusRoutes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    busRoute => busRoute.Id == routeId);

            if (route == null)
            {
                return NotFound("Hat bulunamadı.");
            }

            var stops = await _context.BusRouteStops
                .AsNoTracking()
                .Where(
                    routeStop =>
                        routeStop.RouteId == routeId)
                .OrderBy(
                    routeStop =>
                        routeStop.StopOrder)
                .Select(routeStop =>
                    new BusRouteStopDto
                    {
                        StopId = routeStop.StopId,
                        StopCode =
                            routeStop.BusStop.StopCode,
                        StopName =
                            routeStop.BusStop.StopName,
                        StopOrder =
                            routeStop.StopOrder,
                        LocationDescription =
                            routeStop.BusStop
                                .LocationDescription
                    })
                .ToListAsync();

            return Ok(new
            {
                routeId = route.Id,
                routeCode = route.RouteCode,
                routeName = route.RouteName,
                estimatedDuration =
                    route.EstimatedDuration,
                stops
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(
            CreateBusRouteDto dto)
        {
            var createdRoute =
                await _busRouteService.AddAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdRoute.Id },
                createdRoute
            );
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            UpdateBusRouteDto dto)
        {
            try
            {
                await _busRouteService.UpdateAsync(dto);

                return NoContent();
            }
            catch (Exception exception)
            {
                return NotFound(exception.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted =
                await _busRouteService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Hat bulunamadı.");
            }

            return NoContent();
        }
    }
}
