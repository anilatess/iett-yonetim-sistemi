using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Investigations;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Trips;
using IETT.Entity.DTOs.Inspectors;
using IETT.Entity.DTOs.Certificates;

namespace IETT.Business.Abstract
{
    public enum InvestigationCompletionStatus
    {
        Completed,
        InspectorNotFound,
        InvestigationNotFound,
        NotAssigned,
        AlreadyCompleted,
        MissingTrip,
        MissingDriver
    }

    public class InvestigationDecisionResult
    {
        public InvestigationCompletionStatus Status { get; init; }
        public ComplaintForwardedNotificationDto? Notification { get; init; }

        public static InvestigationDecisionResult Failure(InvestigationCompletionStatus status) =>
            new() { Status = status };
    }

    public enum InspectorGarageScopeStatus
    {
        Success,
        InspectorNotFound,
        GarageNotFound
    }

    public class InspectorGarageScopeResult<T>
    {
        public InspectorGarageScopeStatus Status { get; init; }
        public T? Data { get; init; }

        public static InspectorGarageScopeResult<T> Success(T data) =>
            new() { Status = InspectorGarageScopeStatus.Success, Data = data };

        public static InspectorGarageScopeResult<T> Failure(
            InspectorGarageScopeStatus status) => new() { Status = status };
    }

    public enum InspectorCertificateReviewStatus
    {
        Success,
        InspectorNotFound,
        GarageNotFound,
        CertificateNotFound,
        OutOfGarageScope,
        AlreadyReviewed
    }

    public class InspectorCertificateReviewResult
    {
        public InspectorCertificateReviewStatus Status { get; init; }
        public InspectorCertificateDto? Certificate { get; init; }
    }

    public enum InspectorTripCreationStatus
    {
        Success,
        InspectorNotFound,
        GarageNotFound,
        DriverNotFound,
        DriverOutOfGarageScope,
        DriverUnavailable,
        VehicleNotFound,
        VehicleUnavailable,
        RouteNotFound,
        InvalidTimes,
        DifferentCalendarDays,
        TimeInPast,
        DriverConflict,
        VehicleConflict
    }

    public class InspectorTripCreationResult
    {
        public InspectorTripCreationStatus Status { get; init; }
        public InspectorTripListDto? Trip { get; init; }
        public TripAssignedNotification? Notification { get; init; }

        public static InspectorTripCreationResult Success(
            InspectorTripListDto trip,
            TripAssignedNotification notification) => new()
            {
                Status = InspectorTripCreationStatus.Success,
                Trip = trip,
                Notification = notification
            };

        public static InspectorTripCreationResult Failure(
            InspectorTripCreationStatus status) => new() { Status = status };
    }

    public class TripAssignedNotification
    {
        public int DriverUserId { get; init; }
        public int TripId { get; init; }
        public string RouteCode { get; init; } = string.Empty;
        public string VehicleDoorNumber { get; init; } = string.Empty;
        public DateTime PlannedDepartureDateTime { get; init; }
    }

    public class PerformanceEvaluatedNotification
    {
        public int DriverUserId { get; init; }
        public int PerformanceId { get; init; }
        public int Score { get; init; }
        public DateTime EvaluationDate { get; init; }
    }

    public class InspectorPerformanceCreationResult
    {
        public DriverPerformanceDto? Performance { get; init; }
        public PerformanceEvaluatedNotification? Notification { get; init; }

        public static InspectorPerformanceCreationResult Success(
            DriverPerformanceDto performance,
            PerformanceEvaluatedNotification notification) => new()
            {
                Performance = performance,
                Notification = notification
            };

        public static InspectorPerformanceCreationResult Failure() => new();
    }

    public enum InspectorTripUpdateStatus
    {
        Success,
        InspectorNotFound,
        GarageNotFound,
        TripNotFound,
        TripOutOfGarageScope,
        TripNotEditable,
        DriverNotFound,
        DriverOutOfGarageScope,
        DriverUnavailable,
        VehicleNotFound,
        VehicleUnavailable,
        RouteNotFound,
        InvalidTimes,
        DifferentCalendarDays,
        TimeInPast,
        DriverConflict,
        VehicleConflict
    }

    public class InspectorTripUpdateResult
    {
        public InspectorTripUpdateStatus Status { get; init; }
        public InspectorTripListDto? Trip { get; init; }

        public static InspectorTripUpdateResult Success(InspectorTripListDto trip) =>
            new() { Status = InspectorTripUpdateStatus.Success, Trip = trip };

        public static InspectorTripUpdateResult Failure(
            InspectorTripUpdateStatus status) => new() { Status = status };
    }

    public enum InspectorTripCancellationStatus
    {
        Success,
        InspectorNotFound,
        GarageNotFound,
        TripNotFound,
        TripOutOfGarageScope,
        AlreadyCancelled,
        TripNotCancellable
    }

    public class InspectorTripCancellationResult
    {
        public InspectorTripCancellationStatus Status { get; init; }
        public InspectorTripListDto? Trip { get; init; }

        public static InspectorTripCancellationResult Success(
            InspectorTripListDto trip) => new()
            {
                Status = InspectorTripCancellationStatus.Success,
                Trip = trip
            };

        public static InspectorTripCancellationResult Failure(
            InspectorTripCancellationStatus status) => new() { Status = status };
    }

    public interface IInspectorService
    {
        Task<InspectorGarageScopeResult<InspectorDashboardDto>> GetDashboardAsync(
            int userId);

        Task<InspectorGarageScopeResult<List<InspectorCertificateDto>>>
            GetMyCertificatesAsync(int userId);

        Task<InspectorCertificateReviewResult> ApproveCertificateAsync(
            int userId,
            int certificateId);

        Task<InspectorCertificateReviewResult> RejectCertificateAsync(
            int userId,
            int certificateId,
            string rejectionReason);

        Task<InspectorGarageScopeResult<List<DriverListDto>>> GetMyDriversAsync(
            int userId);

        Task<InspectorGarageScopeResult<List<InspectorTripListDto>>> GetMyTripsAsync(
            int userId);

        Task<InspectorTripCreationResult> CreateMyTripAsync(
            int userId,
            CreateInspectorTripDto dto);

        Task<InspectorTripUpdateResult> UpdateMyTripAsync(
            int userId,
            int tripId,
            UpdateInspectorTripDto dto);

        Task<InspectorTripCancellationResult> CancelMyTripAsync(
            int userId,
            int tripId);

        Task<InspectorPerformanceCreationResult> CreatePerformanceAsync(
            int userId,
            CreateDriverPerformanceDto dto
        );

        Task<List<InspectorPerformanceHistoryDto>> GetMyPerformancesAsync(
            int userId
        );

        Task<List<InspectorInvestigationDto>?> GetMyInvestigationsAsync(
            int userId
        );

        Task<InvestigationDecisionResult> CompleteInvestigationAsync(
            int userId,
            int investigationId,
            CompleteInvestigationDto dto
        );

        Task<InvestigationDecisionResult> DecideInvestigationAsync(
            int userId,
            int investigationId,
            InvestigationDecisionDto dto
        );
    }
}
