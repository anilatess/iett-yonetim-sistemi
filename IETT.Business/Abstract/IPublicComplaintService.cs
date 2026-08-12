using IETT.Entity.DTOs.Complaints;

namespace IETT.Business.Abstract
{
    public interface IPublicComplaintService
    {
        Task<List<PublicComplaintTypeDto>> GetComplaintTypesAsync();

        Task<PublicComplaintTrackingLookupResult> GetByTrackingCodeAsync(
            string trackingCode);

        Task<PublicComplaintOperationResult> CreateAsync(
            CreatePublicComplaintDto dto);
    }

    public enum PublicComplaintTrackingLookupStatus
    {
        Success,
        NotFound,
        DuplicateTrackingCode
    }

    public class PublicComplaintTrackingLookupResult
    {
        public PublicComplaintTrackingLookupStatus Status { get; init; }
        public PublicComplaintTrackingDto? Complaint { get; init; }
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
        VehicleAmbiguous,
        RouteAmbiguous,
        ComplaintTypeNotFound,
        RouteNotFound,
        VehicleNotFound,
        StopNotFound,
        TripNotFound,
        TripAmbiguous,
        InspectorNotAvailable,
        TripRouteMismatch,
        TripVehicleMismatch,
        IncidentDateTimeRequired,
        IncidentDateTimeInFuture,
        DescriptionRequired,
        DescriptionTooLong
    }
}
