using IETT.Entity.DTOs.Complaints;

namespace IETT.Business.Abstract
{
    public interface IPublicComplaintService
    {
        Task<PublicComplaintOperationResult> CreateAsync(
            CreatePublicComplaintDto dto);
    }

    public class PublicComplaintOperationResult
    {
        public PublicComplaintOperationStatus Status { get; init; }
        public PublicComplaintCreatedDto? Complaint { get; init; }

        public static PublicComplaintOperationResult Success(
            PublicComplaintCreatedDto complaint) =>
            new()
            {
                Status = PublicComplaintOperationStatus.Success,
                Complaint = complaint
            };

        public static PublicComplaintOperationResult Failure(
            PublicComplaintOperationStatus status) =>
            new() { Status = status };
    }

    public enum PublicComplaintOperationStatus
    {
        Success,
        ComplaintTypeNotFound,
        RouteNotFound,
        VehicleNotFound,
        StopNotFound,
        TripNotFound,
        TripRouteMismatch,
        TripVehicleMismatch,
        ComplaintDateRequired,
        ComplaintTimeRequired,
        DescriptionRequired,
        DescriptionTooLong
    }
}
