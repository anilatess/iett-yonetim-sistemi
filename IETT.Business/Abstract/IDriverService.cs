using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Complaints;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Trips;

namespace IETT.Business.Abstract
{
    public enum DriverCertificateCreationStatus
    {
        Success,
        DriverNotFound,
        DuplicateCertificateNumber
    }

    public class DriverCertificateCreationResult
    {
        public DriverCertificateCreationStatus Status { get; init; }
        public DriverCertificateDto? Certificate { get; init; }
    }

    public interface IDriverService
    {
        Task<int?> GetMyDriverIdAsync(int userId);

        Task<DriverDashboardDto?> GetDashboardAsync(int userId);

        Task<List<DriverListDto>?> GetAllAsync(int userId, string role);

        Task<List<DriverCertificateDto>> GetMyCertificatesAsync(int userId);

        Task<DriverCertificateCreationResult> CreateMyCertificateAsync(
            int userId,
            CreateDriverCertificateDto dto);

        Task<List<DriverCertificateDto>?> GetCertificatesAsync(
            int requestingUserId,
            string role,
            int driverId);

        Task<List<DriverPerformanceDto>> GetMyPerformancesAsync(int userId);

        Task<List<DriverTripDto>> GetMyTripsAsync(int userId);

        Task<List<DriverComplaintDto>?> GetMyComplaintsAsync(int userId);
    }
}
