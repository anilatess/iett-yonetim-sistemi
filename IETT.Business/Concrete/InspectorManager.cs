using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Investigations;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Trips;
using IETT.Entity.Entities;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using IETT.Business.Utilities;

namespace IETT.Business.Concrete
{
    public class InspectorManager : IInspectorService
    {
        private readonly IETTDbContext _context;

        public InspectorManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<InspectorGarageScopeResult<List<DriverListDto>>>
            GetMyDriversAsync(int userId)
        {
            var scope = await GetGarageScopeAsync(userId);

            if (scope.Status != InspectorGarageScopeStatus.Success)
            {
                return InspectorGarageScopeResult<List<DriverListDto>>
                    .Failure(scope.Status);
            }

            var driverData = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.GarageId == scope.GarageId)
                .OrderBy(driver => driver.User.FirstName)
                .ThenBy(driver => driver.User.LastName)
                .Select(driver => new
                {
                    driver.Id,
                    driver.User.FirstName,
                    driver.User.LastName,
                    driver.PersonnelNumber,
                    driver.Garage.GarageName,
                    driver.Operator.OperatorName,
                    driver.DriverStatusId,
                    driver.HolidayDay
                })
                .ToListAsync();

            var now = DateTime.Now;
            var today = now.Date;
            var currentTime = now.TimeOfDay;
            var driverIds = driverData.Select(driver => driver.Id).ToList();
            var activeTripDriverIds = await _context.Trips
                .AsNoTracking()
                .Where(trip => driverIds.Contains(trip.DriverId)
                    && trip.TripStatus != TripStatusEnum.Cancelled
                    && trip.TripDate.Date == today
                    && trip.DepertureTime <= currentTime
                    && currentTime < trip.ArrivalTime)
                .Select(trip => trip.DriverId)
                .Distinct()
                .ToListAsync();

            var activeDriverIds = activeTripDriverIds.ToHashSet();
            var drivers = driverData.Select(driver => new DriverListDto
            {
                Id = driver.Id,
                FullName = driver.FirstName + " " + driver.LastName,
                MaskedIdentityNumber = string.Empty,
                PersonnelNumber = driver.PersonnelNumber,
                GarageName = driver.GarageName,
                OperatorName = driver.OperatorName,
                DriverStatusName = DriverStatusRules.GetEffectiveStatusName(
                    driver.DriverStatusId,
                    activeDriverIds.Contains(driver.Id)),
                HolidayDay = driver.HolidayDay
            }).ToList();

            return InspectorGarageScopeResult<List<DriverListDto>>.Success(drivers);
        }

        public async Task<InspectorGarageScopeResult<List<InspectorTripListDto>>>
            GetMyTripsAsync(int userId)
        {
            var scope = await GetGarageScopeAsync(userId);

            if (scope.Status != InspectorGarageScopeStatus.Success)
            {
                return InspectorGarageScopeResult<List<InspectorTripListDto>>
                    .Failure(scope.Status);
            }

            var tripData = await _context.Trips
                .AsNoTracking()
                .Where(trip => trip.Driver.GarageId == scope.GarageId)
                .OrderBy(trip => trip.TripDate)
                .ThenBy(trip => trip.DepertureTime)
                .Select(trip => new
                {
                    TripId = trip.Id,
                    DriverFullName = trip.Driver.User.FirstName + " "
                        + trip.Driver.User.LastName,
                    trip.Driver.PersonnelNumber,
                    VehicleDoorNumber = trip.Vehicle.DoorNumber,
                    trip.BusRoute.RouteCode,
                    trip.BusRoute.RouteName,
                    trip.TripDate,
                    trip.DepertureTime,
                    trip.ArrivalTime,
                    trip.TripStatus
                })
                .ToListAsync();

            var trips = tripData.Select(trip => new InspectorTripListDto
            {
                TripId = trip.TripId,
                DriverFullName = trip.DriverFullName,
                PersonnelNumber = trip.PersonnelNumber,
                VehicleDoorNumber = trip.VehicleDoorNumber,
                RouteCode = trip.RouteCode,
                RouteName = trip.RouteName,
                PlannedDepartureDateTime = trip.TripDate.Date + trip.DepertureTime,
                PlannedArrivalDateTime = trip.TripDate.Date + trip.ArrivalTime,
                TripStatus = trip.TripStatus,
                TripStatusName = GetTripStatusName(trip.TripStatus)
            }).ToList();

            return InspectorGarageScopeResult<List<InspectorTripListDto>>.Success(trips);
        }

        public async Task<InspectorTripCreationResult> CreateMyTripAsync(
            int userId,
            CreateInspectorTripDto dto)
        {
            var scope = await GetGarageScopeAsync(userId);

            if (scope.Status == InspectorGarageScopeStatus.InspectorNotFound)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.InspectorNotFound);
            }

            if (scope.Status == InspectorGarageScopeStatus.GarageNotFound)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.GarageNotFound);
            }

            if (dto.PlannedDepartureDateTime >= dto.PlannedArrivalDateTime)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.InvalidTimes);
            }

            if (dto.PlannedDepartureDateTime.Date != dto.PlannedArrivalDateTime.Date)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.DifferentCalendarDays);
            }

            var now = DateTime.Now;
            if (dto.PlannedDepartureDateTime < now
                || dto.PlannedArrivalDateTime < now)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.TimeInPast);
            }

            var driver = await _context.Drivers
                .AsNoTracking()
                .Where(item => item.Id == dto.DriverId)
                .Select(item => new
                {
                    item.GarageId,
                    item.DriverStatusId
                })
                .FirstOrDefaultAsync();

            if (driver is null)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.DriverNotFound);
            }

            if (driver.GarageId != scope.GarageId)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.DriverOutOfGarageScope);
            }

            if (!DriverStatusRules.CanReceiveTrip(driver.DriverStatusId))
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.DriverUnavailable);
            }

            var vehicleStatusId = await _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.Id == dto.VehicleId)
                .Select(vehicle => (int?)vehicle.VehicleStatusId)
                .FirstOrDefaultAsync();

            if (vehicleStatusId is null)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.VehicleNotFound);
            }

            if (vehicleStatusId.Value != (int)VehicleStatusEnum.Active)
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.VehicleUnavailable);
            }

            if (!await _context.BusRoutes.AsNoTracking()
                    .AnyAsync(route => route.Id == dto.RouteId))
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.RouteNotFound);
            }

            var tripDate = dto.PlannedDepartureDateTime.Date;
            var departureTime = dto.PlannedDepartureDateTime.TimeOfDay;
            var arrivalTime = dto.PlannedArrivalDateTime.TimeOfDay;

            var activeTrips = _context.Trips
                .AsNoTracking()
                .Where(trip => trip.TripStatus != TripStatusEnum.Cancelled
                    && trip.TripDate.Date == tripDate
                    && departureTime < trip.ArrivalTime
                    && arrivalTime > trip.DepertureTime);

            if (await activeTrips.AnyAsync(trip => trip.DriverId == dto.DriverId))
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.DriverConflict);
            }

            if (await activeTrips.AnyAsync(trip => trip.VehicleId == dto.VehicleId))
            {
                return InspectorTripCreationResult.Failure(
                    InspectorTripCreationStatus.VehicleConflict);
            }

            var newTrip = new Trip
            {
                DriverId = dto.DriverId,
                VehicleId = dto.VehicleId,
                RouteId = dto.RouteId,
                TripDate = tripDate,
                DepertureTime = departureTime,
                ArrivalTime = arrivalTime,
                TripStatus = TripStatusEnum.Planned
            };

            _context.Trips.Add(newTrip);
            await _context.SaveChangesAsync();

            var createdTrip = await GetInspectorTripAsync(newTrip.Id);
            return InspectorTripCreationResult.Success(createdTrip!);
        }

        public async Task<InspectorTripUpdateResult> UpdateMyTripAsync(
            int userId,
            int tripId,
            UpdateInspectorTripDto dto)
        {
            var scope = await GetGarageScopeAsync(userId);

            if (scope.Status == InspectorGarageScopeStatus.InspectorNotFound)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.InspectorNotFound);
            }

            if (scope.Status == InspectorGarageScopeStatus.GarageNotFound)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.GarageNotFound);
            }

            var trip = await _context.Trips
                .Include(item => item.Driver)
                .FirstOrDefaultAsync(item => item.Id == tripId);

            if (trip is null)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.TripNotFound);
            }

            if (trip.Driver.GarageId != scope.GarageId)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.TripOutOfGarageScope);
            }

            if (trip.TripStatus != TripStatusEnum.Planned)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.TripNotEditable);
            }

            if (dto.PlannedDepartureDateTime >= dto.PlannedArrivalDateTime)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.InvalidTimes);
            }

            if (dto.PlannedDepartureDateTime.Date != dto.PlannedArrivalDateTime.Date)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.DifferentCalendarDays);
            }

            var now = DateTime.Now;
            if (dto.PlannedDepartureDateTime < now
                || dto.PlannedArrivalDateTime < now)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.TimeInPast);
            }

            var driver = await _context.Drivers
                .AsNoTracking()
                .Where(item => item.Id == dto.DriverId)
                .Select(item => new { item.GarageId, item.DriverStatusId })
                .FirstOrDefaultAsync();

            if (driver is null)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.DriverNotFound);
            }

            if (driver.GarageId != scope.GarageId)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.DriverOutOfGarageScope);
            }

            if (!DriverStatusRules.CanReceiveTrip(driver.DriverStatusId))
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.DriverUnavailable);
            }

            var vehicleStatusId = await _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.Id == dto.VehicleId)
                .Select(vehicle => (int?)vehicle.VehicleStatusId)
                .FirstOrDefaultAsync();

            if (vehicleStatusId is null)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.VehicleNotFound);
            }

            if (vehicleStatusId.Value != (int)VehicleStatusEnum.Active)
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.VehicleUnavailable);
            }

            if (!await _context.BusRoutes.AsNoTracking()
                    .AnyAsync(route => route.Id == dto.RouteId))
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.RouteNotFound);
            }

            var tripDate = dto.PlannedDepartureDateTime.Date;
            var departureTime = dto.PlannedDepartureDateTime.TimeOfDay;
            var arrivalTime = dto.PlannedArrivalDateTime.TimeOfDay;

            var conflictingTrips = _context.Trips
                .AsNoTracking()
                .Where(item => item.Id != tripId
                    && item.TripStatus != TripStatusEnum.Cancelled
                    && item.TripDate.Date == tripDate
                    && departureTime < item.ArrivalTime
                    && arrivalTime > item.DepertureTime);

            if (await conflictingTrips.AnyAsync(item => item.DriverId == dto.DriverId))
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.DriverConflict);
            }

            if (await conflictingTrips.AnyAsync(item => item.VehicleId == dto.VehicleId))
            {
                return InspectorTripUpdateResult.Failure(
                    InspectorTripUpdateStatus.VehicleConflict);
            }

            trip.DriverId = dto.DriverId;
            trip.VehicleId = dto.VehicleId;
            trip.RouteId = dto.RouteId;
            trip.TripDate = tripDate;
            trip.DepertureTime = departureTime;
            trip.ArrivalTime = arrivalTime;

            await _context.SaveChangesAsync();

            var updatedTrip = await GetInspectorTripAsync(trip.Id);
            return InspectorTripUpdateResult.Success(updatedTrip!);
        }

        public async Task<InspectorTripCancellationResult> CancelMyTripAsync(
            int userId,
            int tripId)
        {
            var scope = await GetGarageScopeAsync(userId);

            if (scope.Status == InspectorGarageScopeStatus.InspectorNotFound)
            {
                return InspectorTripCancellationResult.Failure(
                    InspectorTripCancellationStatus.InspectorNotFound);
            }

            if (scope.Status == InspectorGarageScopeStatus.GarageNotFound)
            {
                return InspectorTripCancellationResult.Failure(
                    InspectorTripCancellationStatus.GarageNotFound);
            }

            var trip = await _context.Trips
                .Include(item => item.Driver)
                .FirstOrDefaultAsync(item => item.Id == tripId);

            if (trip is null)
            {
                return InspectorTripCancellationResult.Failure(
                    InspectorTripCancellationStatus.TripNotFound);
            }

            if (trip.Driver.GarageId != scope.GarageId)
            {
                return InspectorTripCancellationResult.Failure(
                    InspectorTripCancellationStatus.TripOutOfGarageScope);
            }

            if (trip.TripStatus == TripStatusEnum.Cancelled)
            {
                return InspectorTripCancellationResult.Failure(
                    InspectorTripCancellationStatus.AlreadyCancelled);
            }

            if (trip.TripStatus != TripStatusEnum.Planned)
            {
                return InspectorTripCancellationResult.Failure(
                    InspectorTripCancellationStatus.TripNotCancellable);
            }

            trip.TripStatus = TripStatusEnum.Cancelled;
            await _context.SaveChangesAsync();

            var cancelledTrip = await GetInspectorTripAsync(trip.Id);
            return InspectorTripCancellationResult.Success(cancelledTrip!);
        }

        private async Task<InspectorTripListDto?> GetInspectorTripAsync(int tripId)
        {
            var trip = await _context.Trips
                .AsNoTracking()
                .Where(item => item.Id == tripId)
                .Select(item => new
                {
                    TripId = item.Id,
                    DriverFullName = item.Driver.User.FirstName + " "
                        + item.Driver.User.LastName,
                    item.Driver.PersonnelNumber,
                    VehicleDoorNumber = item.Vehicle.DoorNumber,
                    item.BusRoute.RouteCode,
                    item.BusRoute.RouteName,
                    item.TripDate,
                    item.DepertureTime,
                    item.ArrivalTime,
                    item.TripStatus
                })
                .FirstOrDefaultAsync();

            return trip is null ? null : new InspectorTripListDto
            {
                TripId = trip.TripId,
                DriverFullName = trip.DriverFullName,
                PersonnelNumber = trip.PersonnelNumber,
                VehicleDoorNumber = trip.VehicleDoorNumber,
                RouteCode = trip.RouteCode,
                RouteName = trip.RouteName,
                PlannedDepartureDateTime = trip.TripDate.Date + trip.DepertureTime,
                PlannedArrivalDateTime = trip.TripDate.Date + trip.ArrivalTime,
                TripStatus = trip.TripStatus,
                TripStatusName = GetTripStatusName(trip.TripStatus)
            };
        }

        private async Task<(InspectorGarageScopeStatus Status, int GarageId)>
            GetGarageScopeAsync(int userId)
        {
            var inspector = await _context.Inspectors
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .Select(item => new { item.GarageId })
                .FirstOrDefaultAsync();

            if (inspector is null)
            {
                return (InspectorGarageScopeStatus.InspectorNotFound, 0);
            }

            var garageExists = await _context.Garages
                .AsNoTracking()
                .AnyAsync(garage => garage.Id == inspector.GarageId);

            return garageExists
                ? (InspectorGarageScopeStatus.Success, inspector.GarageId)
                : (InspectorGarageScopeStatus.GarageNotFound, 0);
        }

        private static string GetTripStatusName(TripStatusEnum status) => status switch
        {
            TripStatusEnum.Planned => "Planlandı",
            TripStatusEnum.InProgress => "Devam Ediyor",
            TripStatusEnum.Completed => "Tamamlandı",
            TripStatusEnum.Cancelled => "İptal Edildi",
            _ => "Bilinmiyor"
        };

        public async Task<DriverPerformanceDto?> CreatePerformanceAsync(
            int userId,
            CreateDriverPerformanceDto dto)
        {
            var inspector = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => new
                {
                    inspector.Id,
                    inspector.GarageId,
                    inspector.User.FirstName,
                    inspector.User.LastName
                })
                .FirstOrDefaultAsync();

            if (inspector is null)
            {
                return null;
            }

            var driverExists = await _context.Drivers
                .AsNoTracking()
                .AnyAsync(driver =>
                    driver.Id == dto.DriverId
                    && driver.GarageId == inspector.GarageId);

            if (!driverExists)
            {
                return null;
            }

            var performance = new DriverPerformance
            {
                DriverId = dto.DriverId,
                InspectorId = inspector.Id,
                Score = dto.Score,
                PerformanceComment = dto.PerformanceComment ?? string.Empty,
                EvaluationDate = DateTime.Now
            };

            _context.DriverPerformances.Add(performance);
            await _context.SaveChangesAsync();

            return new DriverPerformanceDto
            {
                Id = performance.Id,
                Score = performance.Score,
                PerformanceComment = performance.PerformanceComment ?? string.Empty,
                EvaluationDate = performance.EvaluationDate,
                InspectorFullName = inspector.FirstName + " " + inspector.LastName
            };
        }

        public async Task<List<InspectorPerformanceHistoryDto>> GetMyPerformancesAsync(
            int userId)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return new List<InspectorPerformanceHistoryDto>();
            }

            return await _context.DriverPerformances
                .AsNoTracking()
                .Where(performance => performance.InspectorId == inspectorId.Value)
                .OrderByDescending(performance => performance.EvaluationDate)
                .Select(performance => new InspectorPerformanceHistoryDto
                {
                    Id = performance.Id,
                    DriverId = performance.DriverId,
                    DriverFullName = performance.Driver.User.FirstName
                        + " " + performance.Driver.User.LastName,
                    PersonnelNumber = performance.Driver.PersonnelNumber,
                    Score = performance.Score,
                    PerformanceComment = performance.PerformanceComment ?? string.Empty,
                    EvaluationDate = performance.EvaluationDate
                })
                .ToListAsync();
        }

        public async Task<List<InspectorInvestigationDto>?> GetMyInvestigationsAsync(
            int userId)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return null;
            }

            return await _context.Investigations
                .AsNoTracking()
                .Where(investigation => investigation.InspectorId == inspectorId.Value)
                .OrderBy(investigation => investigation.ClosedDate.HasValue)
                .ThenByDescending(investigation => investigation.CreatedDate)
                .Select(investigation => new InspectorInvestigationDto
                {
                    Id = investigation.Id,
                    ComplaintId = investigation.ComplaintId,
                    TrackingCode = investigation.Complaint.TrackingCode,
                    ComplaintTypeName = investigation.Complaint.ComplaintType.ComplaintTypeName,
                    ComplaintDescription = investigation.Complaint.ComplaintDescription,
                    ComplaintCreatedDate = investigation.Complaint.CreatedDate,
                    InvestigationTitle = investigation.InvestigationTitle,
                    InvestigationDescription = investigation.InvestigationDescription ?? string.Empty,
                    InvestigationResult = investigation.InvestigationResult ?? string.Empty,
                    InvestigationCreatedDate = investigation.CreatedDate,
                    ClosedDate = investigation.ClosedDate,
                    Status = investigation.ClosedDate.HasValue ? "Tamamlandı" : "Devam Ediyor",
                    DriverId = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.DriverId,
                    DriverFullName = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.Driver.User.FirstName
                            + " " + investigation.Complaint.Trip.Driver.User.LastName,
                    PersonnelNumber = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.Driver.PersonnelNumber,
                    VehicleId = investigation.Complaint.VehicleId,
                    VehicleDoorNumber = investigation.Complaint.Vehicle.DoorNumber,
                    RouteId = investigation.Complaint.RouteId,
                    RouteCode = investigation.Complaint.BusRoute.RouteCode,
                    RouteName = investigation.Complaint.BusRoute.RouteName,
                    StopId = investigation.Complaint.StopId,
                    StopCode = investigation.Complaint.BusStop.StopCode,
                    StopName = investigation.Complaint.BusStop.StopName,
                    TripId = investigation.Complaint.TripId,
                    TripDate = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.TripDate,
                    DepertureTime = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.DepertureTime,
                    ArrivalTime = investigation.Complaint.Trip == null
                        ? null
                        : investigation.Complaint.Trip.ArrivalTime
                })
                .ToListAsync();
        }

        public async Task<InvestigationCompletionStatus> CompleteInvestigationAsync(
            int userId,
            int investigationId,
            CompleteInvestigationDto dto)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return InvestigationCompletionStatus.NotFound;
            }

            var investigation = await _context.Investigations
                .Include(item => item.Complaint)
                .FirstOrDefaultAsync(item =>
                    item.Id == investigationId
                    && item.InspectorId == inspectorId.Value);

            if (investigation is null)
            {
                return InvestigationCompletionStatus.NotFound;
            }

            if (investigation.ClosedDate.HasValue)
            {
                return InvestigationCompletionStatus.AlreadyCompleted;
            }

            investigation.InvestigationResult = dto.InvestigationResult.Trim();
            investigation.ClosedDate = DateTime.Now;

            var hasAnotherOpenInvestigation = await _context.Investigations
                .AsNoTracking()
                .AnyAsync(item =>
                    item.ComplaintId == investigation.ComplaintId
                    && item.Id != investigation.Id
                    && !item.ClosedDate.HasValue);

            if (!hasAnotherOpenInvestigation)
            {
                investigation.Complaint.ComplaintStatus = ComplaintStatusEnum.Resolved;
            }

            await _context.SaveChangesAsync();
            return InvestigationCompletionStatus.Completed;
        }
    }
}
