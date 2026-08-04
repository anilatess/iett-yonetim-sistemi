using IETT.Entity.DTOs.Trips;

namespace IETT.Business.Abstract
{
    public interface ITripService
    {
        Task<List<TripListDto>?> GetAllAsync(int userId, string role);
        Task<TripListDto?> GetByIdAsync(int id, int userId, string role);
        Task<TripOperationResult> CreateAsync(
            CreateTripDto dto,
            int userId,
            string role);
        Task<TripOperationResult> UpdateAsync(
            int id,
            UpdateTripDto dto,
            int userId,
            string role);
    }

    public class TripOperationResult
    {
        public TripOperationStatus Status { get; init; }
        public TripListDto? Trip { get; init; }

        public static TripOperationResult Success(TripListDto? trip = null) =>
            new() { Status = TripOperationStatus.Success, Trip = trip };

        public static TripOperationResult Failure(TripOperationStatus status) =>
            new() { Status = status };
    }

    public enum TripOperationStatus
    {
        Success,
        InspectorNotFound,
        DriverNotFoundOrOutOfScope,
        TripNotFoundOrOutOfScope,
        VehicleNotFound,
        RouteNotFound,
        InvalidTripDate,
        InvalidTripStatus,
        InvalidTimes
    }
}
