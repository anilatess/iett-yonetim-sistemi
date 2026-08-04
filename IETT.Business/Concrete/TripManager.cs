using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Trips;
using IETT.Entity.Entities;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IETT.Business.Concrete
{
    public class TripManager : ITripService
    {
        private readonly IETTDbContext _context;

        public TripManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<List<TripListDto>?> GetAllAsync(
            int userId,
            string role)
        {
            var garageResult = await GetGarageScopeAsync(userId, role);

            if (!garageResult.IsValid)
            {
                return null;
            }

            return await BuildScopedQuery(garageResult.GarageId)
                .OrderBy(trip => trip.TripDate)
                .ThenBy(trip => trip.DepertureTime)
                .Select(TripProjection)
                .ToListAsync();
        }

        public async Task<TripListDto?> GetByIdAsync(
            int id,
            int userId,
            string role)
        {
            var garageResult = await GetGarageScopeAsync(userId, role);

            if (!garageResult.IsValid)
            {
                return null;
            }

            return await BuildScopedQuery(garageResult.GarageId)
                .Where(trip => trip.Id == id)
                .Select(TripProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<TripOperationResult> CreateAsync(
            CreateTripDto dto,
            int userId,
            string role)
        {
            var validation = await ValidateAsync(
                dto.DriverId,
                dto.VehicleId,
                dto.RouteId,
                dto.TripDate,
                dto.DepertureTime,
                dto.ArrivalTime,
                dto.TripStatus,
                userId,
                role);

            if (validation.Status != TripOperationStatus.Success)
            {
                return validation;
            }

            var trip = new Trip
            {
                DriverId = dto.DriverId,
                VehicleId = dto.VehicleId,
                RouteId = dto.RouteId,
                TripDate = dto.TripDate,
                DepertureTime = dto.DepertureTime,
                ArrivalTime = dto.ArrivalTime,
                TripStatus = dto.TripStatus
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            var createdTrip = await GetByIdAsync(trip.Id, userId, role);
            return TripOperationResult.Success(createdTrip);
        }

        public async Task<TripOperationResult> UpdateAsync(
            int id,
            UpdateTripDto dto,
            int userId,
            string role)
        {
            var garageResult = await GetGarageScopeAsync(userId, role);

            if (!garageResult.IsValid)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.InspectorNotFound);
            }

            var tripQuery = _context.Trips.AsQueryable();

            if (garageResult.GarageId.HasValue)
            {
                tripQuery = tripQuery.Where(trip =>
                    trip.Driver.GarageId == garageResult.GarageId.Value);
            }

            var trip = await tripQuery.FirstOrDefaultAsync(trip => trip.Id == id);

            if (trip is null)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.TripNotFoundOrOutOfScope);
            }

            var validation = await ValidateAsync(
                dto.DriverId,
                dto.VehicleId,
                dto.RouteId,
                dto.TripDate,
                dto.DepertureTime,
                dto.ArrivalTime,
                dto.TripStatus,
                userId,
                role,
                garageResult.GarageId);

            if (validation.Status != TripOperationStatus.Success)
            {
                return validation;
            }

            trip.DriverId = dto.DriverId;
            trip.VehicleId = dto.VehicleId;
            trip.RouteId = dto.RouteId;
            trip.TripDate = dto.TripDate;
            trip.DepertureTime = dto.DepertureTime;
            trip.ArrivalTime = dto.ArrivalTime;
            trip.TripStatus = dto.TripStatus;

            await _context.SaveChangesAsync();
            return TripOperationResult.Success();
        }

        private async Task<TripOperationResult> ValidateAsync(
            int driverId,
            int vehicleId,
            int routeId,
            DateTime tripDate,
            TimeSpan depertureTime,
            TimeSpan arrivalTime,
            TripStatusEnum tripStatus,
            int userId,
            string role,
            int? knownGarageId = null)
        {
            if (driverId < 1)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.DriverNotFoundOrOutOfScope);
            }

            if (vehicleId < 1)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.VehicleNotFound);
            }

            if (routeId < 1)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.RouteNotFound);
            }

            if (tripDate == default)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.InvalidTripDate);
            }

            if (!Enum.IsDefined(tripStatus))
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.InvalidTripStatus);
            }

            if (arrivalTime <= depertureTime)
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.InvalidTimes);
            }

            int? garageId = knownGarageId;

            if (role == "Inspector" && !garageId.HasValue)
            {
                var garageResult = await GetGarageScopeAsync(userId, role);

                if (!garageResult.IsValid)
                {
                    return TripOperationResult.Failure(
                        TripOperationStatus.InspectorNotFound);
                }

                garageId = garageResult.GarageId;
            }

            var driverQuery = _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.Id == driverId);

            if (garageId.HasValue)
            {
                driverQuery = driverQuery.Where(driver =>
                    driver.GarageId == garageId.Value);
            }

            if (!await driverQuery.AnyAsync())
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.DriverNotFoundOrOutOfScope);
            }

            if (!await _context.Vehicles
                    .AsNoTracking()
                    .AnyAsync(vehicle => vehicle.Id == vehicleId))
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.VehicleNotFound);
            }

            if (!await _context.BusRoutes
                    .AsNoTracking()
                    .AnyAsync(route => route.Id == routeId))
            {
                return TripOperationResult.Failure(
                    TripOperationStatus.RouteNotFound);
            }

            return TripOperationResult.Success();
        }

        private async Task<GarageScopeResult> GetGarageScopeAsync(
            int userId,
            string role)
        {
            if (role != "Inspector")
            {
                return new GarageScopeResult(true, null);
            }

            var garageId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.GarageId)
                .FirstOrDefaultAsync();

            return new GarageScopeResult(garageId.HasValue, garageId);
        }

        private IQueryable<Trip> BuildScopedQuery(int? garageId)
        {
            var query = _context.Trips.AsNoTracking();

            if (garageId.HasValue)
            {
                query = query.Where(trip =>
                    trip.Driver.GarageId == garageId.Value);
            }

            return query;
        }

        private static readonly Expression<Func<Trip, TripListDto>>
            TripProjection = trip => new TripListDto
            {
                Id = trip.Id,
                DriverId = trip.DriverId,
                DriverFullName = trip.Driver.User.FirstName
                    + " " + trip.Driver.User.LastName,
                PersonnelNumber = trip.Driver.PersonnelNumber,
                GarageId = trip.Driver.GarageId,
                GarageName = trip.Driver.Garage.GarageName,
                VehicleId = trip.VehicleId,
                VehicleDoorNumber = trip.Vehicle.DoorNumber,
                RouteId = trip.RouteId,
                RouteCode = trip.BusRoute.RouteCode,
                RouteName = trip.BusRoute.RouteName,
                TripDate = trip.TripDate,
                DepertureTime = trip.DepertureTime,
                ArrivalTime = trip.ArrivalTime,
                TripStatus = trip.TripStatus,
                TripStatusName = trip.TripStatus == TripStatusEnum.Planned
                    ? "Planlandı"
                    : trip.TripStatus == TripStatusEnum.InProgress
                        ? "Devam Ediyor"
                        : trip.TripStatus == TripStatusEnum.Completed
                            ? "Tamamlandı"
                            : trip.TripStatus == TripStatusEnum.Cancelled
                                ? "İptal Edildi"
                                : "Bilinmiyor"
            };

        private readonly record struct GarageScopeResult(
            bool IsValid,
            int? GarageId);
    }
}
