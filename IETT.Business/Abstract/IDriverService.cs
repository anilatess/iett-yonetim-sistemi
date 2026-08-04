using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Trips;

namespace IETT.Business.Abstract
{
    public interface IDriverService
    {
        Task<List<DriverListDto>?> GetAllAsync(int userId, string role);

        Task<List<DriverCertificateDto>> GetMyCertificatesAsync(int userId);

        Task<List<DriverCertificateDto>?> GetCertificatesAsync(
            int requestingUserId,
            string role,
            int driverId);

        Task<List<DriverPerformanceDto>> GetMyPerformancesAsync(int userId);

        Task<List<DriverTripDto>> GetMyTripsAsync(int userId);
    }
}
