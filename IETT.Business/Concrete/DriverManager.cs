using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Drivers;
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

        public async Task<List<DriverListDto>?> GetAllAsync(
            int userId,
            string role)
        {
            int? garageId = null;

            if (role == "Inspector")
            {
                garageId = await _context.Inspectors
                    .AsNoTracking()
                    .Where(inspector => inspector.UserId == userId)
                    .Select(inspector => (int?)inspector.GarageId)
                    .FirstOrDefaultAsync();

                if (garageId is null)
                {
                    return null;
                }
            }

            var query = _context.Drivers.AsNoTracking();

            if (garageId.HasValue)
            {
                query = query.Where(driver =>
                    driver.GarageId == garageId.Value);
            }

            var driverData = await query
                .Select(driver => new
                {
                    driver.Id,
                    driver.PersonnelNumber,
                    driver.HolidayDay,
                    driver.User.FirstName,
                    driver.User.LastName,
                    driver.User.IdentityNumber,
                    driver.Garage.GarageName,
                    driver.Operator.OperatorName,
                    DriverStatusName = driver.DriverStatus.StatusName
                })
                .ToListAsync();

            return driverData.Select(driver => new DriverListDto
            {
                Id = driver.Id,
                FullName = driver.FirstName + " " + driver.LastName,
                MaskedIdentityNumber =
                    MaskIdentityNumber(driver.IdentityNumber),
                PersonnelNumber = driver.PersonnelNumber,
                GarageName = driver.GarageName,
                OperatorName = driver.OperatorName,
                DriverStatusName = driver.DriverStatusName,
                HolidayDay = driver.HolidayDay.ToString()
            }).ToList();
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

            return await GetCertificateDtosAsync(driverId.Value);
        }

        public async Task<List<DriverCertificateDto>?> GetCertificatesAsync(
            int requestingUserId,
            string role,
            int driverId)
        {
            var drivers = _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.Id == driverId);

            if (role == "Inspector")
            {
                var garageId = await _context.Inspectors
                    .AsNoTracking()
                    .Where(inspector => inspector.UserId == requestingUserId)
                    .Select(inspector => (int?)inspector.GarageId)
                    .FirstOrDefaultAsync();

                if (garageId is null)
                {
                    return null;
                }

                drivers = drivers.Where(driver => driver.GarageId == garageId.Value);
            }

            if (!await drivers.AnyAsync())
            {
                return null;
            }

            return await GetCertificateDtosAsync(driverId);
        }

        private async Task<List<DriverCertificateDto>> GetCertificateDtosAsync(int driverId)
        {
            var certificates = await _context.DriverCertificates
                .AsNoTracking()
                .Where(certificate => certificate.DriverId == driverId)
                .OrderBy(certificate => certificate.ExpiryDate)
                .Select(certificate => new
                {
                    certificate.Id,
                    certificate.CertificateNumber,
                    certificate.ExpiryDate
                })
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

        private static string MaskIdentityNumber(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return "***********";
            }

            if (identityNumber.Length < 4)
            {
                return new string('*', identityNumber.Length);
            }

            return identityNumber[..2]
                + new string('*', identityNumber.Length - 4)
                + identityNumber[^2..];
        }
    }
}
