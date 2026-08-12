using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Complaints;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Trips;
using IETT.Entity.DTOs.Investigations;

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

    public enum DriverExplanationStatus
    {
        Success, DriverNotFound, InvestigationNotFound, NotAssigned,
        InvalidProcessStage, AlreadySubmitted, AlreadyCompleted
    }

    public class DriverExplanationResult
    {
        public DriverExplanationStatus Status { get; init; }
        public int? InspectorUserId { get; init; }
        public int InvestigationId { get; init; }
        public int ComplaintId { get; init; }
        public DateTime? SubmittedDate { get; init; }
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
        Task<DriverExplanationResult> SubmitExplanationAsync(
            int userId, int investigationId, DriverExplanationDto dto);
    }
}
