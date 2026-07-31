using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Performances;
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

        public async Task<List<DriverCertificateDto>> GetMyCertificatesAsync(int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new List<DriverCertificateDto>();
            }

            var certificates = await _context.DriverCertificates
                .AsNoTracking()
                .Where(certificate => certificate.DriverId == driverId.Value)
                .OrderBy(certificate => certificate.ExpiryDate)
                .ToListAsync();

            var today = DateTime.Today;

            return certificates.Select(certificate =>
            {
                var remainingDays = (certificate.ExpiryDate.Date - today).Days;
                var status = certificate.ExpiryDate.Date < today
                    ? "Expired"
                    : remainingDays <= 30
                        ? "ExpiringSoon"
                        : "Valid";

                return new DriverCertificateDto
                {
                    Id = certificate.Id,
                    CertificateNumber = certificate.CertificateNumber,
                    ExpiryDate = certificate.ExpiryDate,
                    RemainingDays = remainingDays,
                    Status = status
                };
            }).ToList();
        }

        public async Task<List<DriverPerformanceDto>> GetMyPerformancesAsync(int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new List<DriverPerformanceDto>();
            }

            return await _context.DriverPerformances
                .AsNoTracking()
                .Where(performance => performance.DriverId == driverId.Value)
                .OrderByDescending(performance => performance.EvaluationDate)
                .Select(performance => new DriverPerformanceDto
                {
                    Id = performance.Id,
                    Score = performance.Score,
                    PerformanceComment = performance.PerformanceComment ?? string.Empty,
                    EvaluationDate = performance.EvaluationDate,
                    InspectorFullName = performance.Inspector.User.FirstName
                        + " " + performance.Inspector.User.LastName
                })
                .ToListAsync();
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
