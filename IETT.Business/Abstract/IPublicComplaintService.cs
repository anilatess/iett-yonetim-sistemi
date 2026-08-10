using IETT.Entity.DTOs.Complaints;

namespace IETT.Business.Abstract
{
    public interface IPublicComplaintService
    {
        Task<List<PublicComplaintTypeDto>> GetComplaintTypesAsync();

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
        DoorNumberRequired,
        RouteCodeRequired,
        VehicleAmbiguous,
        RouteAmbiguous,
        ComplaintTypeNotFound,
        RouteNotFound,
        VehicleNotFound,
        StopNotFound,
        TripNotFound,
        TripRouteMismatch,
        TripVehicleMismatch,
        IncidentDateTimeRequired,
        IncidentDateTimeInFuture,
        DescriptionRequired,
        DescriptionTooLong
    }
}
