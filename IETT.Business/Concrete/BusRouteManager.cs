using IETT.Business.Abstract;
using IETT.DataAccess.Abstract;
using IETT.Entity.DTOs.BusRouteDtos;
using IETT.Entity.Entities;

namespace IETT.Business.Concrete
{
    public class BusRouteManager : IBusRouteService
    {
        private readonly IBusRouteDal _busRouteDal;

        public BusRouteManager(IBusRouteDal busRouteDal)
        {
            _busRouteDal = busRouteDal;
        }

        public async Task<List<BusRouteDto>> GetAllAsync()
        {
            var routes = await _busRouteDal.GetListAsync();

            return routes.Select(route => new BusRouteDto
            {
                Id = route.Id,
                RouteCode = route.RouteCode,
                RouteName = route.RouteName,
                EstimatedDuration = route.EstimatedDuration
            }).ToList();
        }

        public async Task<BusRouteDto?> GetByIdAsync(int id)
        {
            var route = await _busRouteDal.GetAsync(
                route => route.Id == id
            );

            if (route == null)
            {
                return null;
            }

            return new BusRouteDto
            {
                Id = route.Id,
                RouteCode = route.RouteCode,
                RouteName = route.RouteName,
                EstimatedDuration = route.EstimatedDuration
            };
        }

        public async Task<BusRouteDto> AddAsync(CreateBusRouteDto dto)
        {
            var route = new BusRoute
            {
                RouteCode = dto.RouteCode.Trim(),
                RouteName = dto.RouteName.Trim(),
                EstimatedDuration = dto.EstimatedDuration
            };

            await _busRouteDal.AddAsync(route);

            return new BusRouteDto
            {
                Id = route.Id,
                RouteCode = route.RouteCode,
                RouteName = route.RouteName,
                EstimatedDuration = route.EstimatedDuration
            };
        }

        public async Task UpdateAsync(UpdateBusRouteDto dto)
        {
            var route = await _busRouteDal.GetAsync(
                route => route.Id == dto.Id
            );

            if (route == null)
            {
                throw new Exception("Hat bulunamadı.");
            }

            route.RouteCode = dto.RouteCode.Trim();
            route.RouteName = dto.RouteName.Trim();
            route.EstimatedDuration = dto.EstimatedDuration;

            _busRouteDal.Update(route);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var route = await _busRouteDal.GetAsync(
                route => route.Id == id
            );

            if (route == null)
            {
                return false;
            }

            _busRouteDal.Delete(route);

            return true;
        }
    }
}