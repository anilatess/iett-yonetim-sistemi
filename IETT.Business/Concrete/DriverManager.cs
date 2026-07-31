using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Trips;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class DriverManager : IDriverService
    {
        private readonly IETTDbContext _context;

        public DriverManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<List<DriverTripDto>> GetMyTripsAsync(int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new List<DriverTripDto>();
            }

            return await _context.Trips
                .AsNoTracking()
                .Where(trip => trip.DriverId == driverId.Value)
                .OrderBy(trip => trip.TripDate)
                .ThenBy(trip => trip.DepertureTime)
                .Select(trip => new DriverTripDto
                {
                    Id = trip.Id,
                    TripDate = trip.TripDate,
                    RouteCode = trip.BusRoute.RouteCode,
                    RouteName = trip.BusRoute.RouteName,
                    VehicleDoorNumber = trip.Vehicle.DoorNumber,
                    DepertureTime = trip.DepertureTime,
                    ArrivalTime = trip.ArrivalTime,
                    TripStatus = trip.TripStatus.ToString()
                })
                .ToListAsync();
        }
    }
}
