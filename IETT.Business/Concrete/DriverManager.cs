using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Complaints;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Trips;
using IETT.Entity.Enums;
using IETT.Entity.Entities;
using IETT.Business.Utilities;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class DriverManager : IDriverService
    {
        private readonly IETTDbContext _context;

        public DriverManager(IETTDbContext context)
        {
            _context = context;
        }

        public Task<int?> GetMyDriverIdAsync(int userId) => _context.Drivers
            .AsNoTracking()
            .Where(driver => driver.UserId == userId)
            .Select(driver => (int?)driver.Id)
            .FirstOrDefaultAsync();

        public async Task<DriverDashboardDto?> GetDashboardAsync(int userId)
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .Select(item => new
                {
                    item.Id,
                    FullName = item.User.FirstName + " " + item.User.LastName,
                    item.PersonnelNumber,
                    item.Garage.GarageName,
                    item.Operator.OperatorName,
                    item.DriverStatusId,
                    item.HolidayDay
                })
                .FirstOrDefaultAsync();

            if (driver is null)
            {
                return null;
            }

            var now = DateTime.Now;
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            var currentTime = now.TimeOfDay;

            var hasActiveTrip = await _context.Trips
                .AsNoTracking()
                .AnyAsync(trip => trip.DriverId == driver.Id
                    && trip.TripDate >= today
                    && trip.TripDate < tomorrow
                    && trip.TripStatus != TripStatusEnum.Cancelled
                    && trip.DepertureTime <= currentTime
                    && currentTime < trip.ArrivalTime);

            var tripData = await _context.Trips
                .AsNoTracking()
                .Where(trip => trip.DriverId == driver.Id
                    && trip.TripDate >= today
                    && trip.TripDate < tomorrow)
                .OrderBy(trip => trip.DepertureTime)
                .Select(trip => new
                {
                    trip.Id,
                    VehicleDoorNumber = trip.Vehicle.DoorNumber,
                    trip.BusRoute.RouteCode,
                    trip.BusRoute.RouteName,
                    trip.TripDate,
                    trip.DepertureTime,
                    trip.ArrivalTime,
                    trip.TripStatus
                })
                .ToListAsync();

            var complaints = await _context.Complaints
                .AsNoTracking()
                .Where(complaint => complaint.TripId.HasValue
                    && complaint.Trip != null
                    && complaint.Trip.DriverId == driver.Id)
                .OrderByDescending(complaint => complaint.CreatedDate)
                .Select(complaint => new DriverDashboardComplaintDto
                {
                    ComplaintId = complaint.Id,
                    TrackingCode = complaint.TrackingCode,
                    ComplaintTypeName = complaint.ComplaintType.ComplaintTypeName,
                    ComplaintDescription = complaint.ComplaintDescription,
                    CreatedDate = complaint.CreatedDate,
                    ComplaintStatusName = complaint.ComplaintStatus == ComplaintStatusEnum.Pending
                        ? "Beklemede"
                        : complaint.ComplaintStatus == ComplaintStatusEnum.UnderReview
                            ? "İnceleniyor"
                            : complaint.ComplaintStatus == ComplaintStatusEnum.Resolved
                                ? "Çözüldü"
                                : complaint.ComplaintStatus == ComplaintStatusEnum.Rejected
                                    ? "Reddedildi"
                                    : "Bilinmiyor",
                    RouteCode = complaint.BusRoute.RouteCode,
                    VehicleDoorNumber = complaint.Vehicle.DoorNumber,
                    TripId = complaint.TripId!.Value
                })
                .ToListAsync();

            return new DriverDashboardDto
            {
                Driver = new DriverDashboardInfoDto
                {
                    DriverId = driver.Id,
                    FullName = driver.FullName,
                    PersonnelNumber = driver.PersonnelNumber,
                    GarageName = driver.GarageName,
                    OperatorName = driver.OperatorName,
                    DriverStatusName = DriverStatusRules.GetEffectiveStatusName(
                        driver.DriverStatusId,
                        hasActiveTrip),
                    HolidayDay = driver.HolidayDay
                },
                TodayTrips = tripData.Select(trip => new DriverDashboardTripDto
                {
                    TripId = trip.Id,
                    VehicleDoorNumber = trip.VehicleDoorNumber,
                    RouteCode = trip.RouteCode,
                    RouteName = trip.RouteName,
                    PlannedDepartureDateTime = trip.TripDate.Date + trip.DepertureTime,
                    PlannedArrivalDateTime = trip.TripDate.Date + trip.ArrivalTime,
                    TripStatus = trip.TripStatus,
                    TripStatusName = GetTripStatusName(trip.TripStatus)
                }).ToList(),
                Complaints = complaints
            };
        }

        private static string GetTripStatusName(TripStatusEnum status) => status switch
        {
            TripStatusEnum.Planned => "Planlandı",
            TripStatusEnum.InProgress => "Devam Ediyor",
            TripStatusEnum.Completed => "Tamamlandı",
            TripStatusEnum.Cancelled => "İptal Edildi",
            _ => "Bilinmiyor"
        };

        public async Task<List<DriverListDto>?> GetAllAsync(
            int userId,
            string role)
        {
            int? garageId = null;

            if (role == "Inspector")
            {
                garageId = await _context.Inspectors
                    .AsNoTracking()
                    .Where(inspector => inspector.UserId == userId)
                    .Select(inspector => (int?)inspector.GarageId)
                    .FirstOrDefaultAsync();

                if (garageId is null)
                {
                    return null;
                }
            }

            var query = _context.Drivers.AsNoTracking();

            if (garageId.HasValue)
            {
                query = query.Where(driver =>
                    driver.GarageId == garageId.Value);
            }

            var driverData = await query
                .Select(driver => new
                {
                    driver.Id,
                    driver.PersonnelNumber,
                    driver.HolidayDay,
                    driver.User.FirstName,
                    driver.User.LastName,
                    driver.User.IdentityNumber,
                    driver.Garage.GarageName,
                    driver.Operator.OperatorName,
                    driver.DriverStatusId
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

            return driverData.Select(driver => new DriverListDto
            {
                Id = driver.Id,
                FullName = driver.FirstName + " " + driver.LastName,
                MaskedIdentityNumber =
                    MaskIdentityNumber(driver.IdentityNumber),
                PersonnelNumber = driver.PersonnelNumber,
                GarageName = driver.GarageName,
                OperatorName = driver.OperatorName,
                DriverStatusName = DriverStatusRules.GetEffectiveStatusName(
                    driver.DriverStatusId,
                    activeDriverIds.Contains(driver.Id)),
                HolidayDay = driver.HolidayDay.ToString()
            }).ToList();
        }

        public async Task<List<DriverCertificateDto>> GetMyCertificatesAsync(int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new List<DriverCertificateDto>();
            }

            return await GetCertificateDtosAsync(driverId.Value);
        }

        public async Task<DriverCertificateCreationResult> CreateMyCertificateAsync(
            int userId,
            CreateDriverCertificateDto dto)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new DriverCertificateCreationResult
                {
                    Status = DriverCertificateCreationStatus.DriverNotFound
                };
            }

            var normalizedNumber = dto.CertificateNumber.Trim();
            var duplicateExists = await _context.DriverCertificates
                .AsNoTracking()
                .AnyAsync(certificate => certificate.DriverId == driverId.Value
                    && certificate.CertificateNumber == normalizedNumber);

            if (duplicateExists)
            {
                return new DriverCertificateCreationResult
                {
                    Status = DriverCertificateCreationStatus.DuplicateCertificateNumber
                };
            }

            var certificate = new DriverCertificate
            {
                DriverId = driverId.Value,
                CertificateType = dto.CertificateType.Trim(),
                CertificateNumber = normalizedNumber,
                IssueDate = dto.IssueDate.Date,
                ExpiryDate = dto.ExpiryDate.Date,
                FilePath = dto.FilePath,
                OriginalFileName = dto.OriginalFileName,
                ApprovalStatus = CertificateApprovalStatusEnum.Pending,
                ReviewedByInspectorId = null,
                ReviewedDate = null,
                RejectionReason = null
            };

            _context.DriverCertificates.Add(certificate);
            await _context.SaveChangesAsync();

            return new DriverCertificateCreationResult
            {
                Status = DriverCertificateCreationStatus.Success,
                Certificate = MapCertificate(certificate)
            };
        }

        public async Task<List<DriverCertificateDto>?> GetCertificatesAsync(
            int requestingUserId,
            string role,
            int driverId)
        {
            var drivers = _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.Id == driverId);

            if (role == "Inspector")
            {
                var garageId = await _context.Inspectors
                    .AsNoTracking()
                    .Where(inspector => inspector.UserId == requestingUserId)
                    .Select(inspector => (int?)inspector.GarageId)
                    .FirstOrDefaultAsync();

                if (garageId is null)
                {
                    return null;
                }

                drivers = drivers.Where(driver => driver.GarageId == garageId.Value);
            }

            if (!await drivers.AnyAsync())
            {
                return null;
            }

            return await GetCertificateDtosAsync(driverId);
        }

        private async Task<List<DriverCertificateDto>> GetCertificateDtosAsync(int driverId)
        {
            var certificates = await _context.DriverCertificates
                .AsNoTracking()
                .Where(certificate => certificate.DriverId == driverId)
                .OrderBy(certificate => certificate.ExpiryDate)
                .Select(certificate => new
                {
                    certificate.Id,
                    certificate.CertificateType,
                    certificate.CertificateNumber,
                    certificate.IssueDate,
                    certificate.ExpiryDate,
                    certificate.FilePath,
                    certificate.OriginalFileName,
                    certificate.ApprovalStatus,
                    certificate.ReviewedDate,
                    certificate.RejectionReason
                })
                .ToListAsync();

            var today = DateTime.Today;

            return certificates.Select(certificate => new DriverCertificateDto
                {
                    Id = certificate.Id,
                    CertificateType = certificate.CertificateType,
                    CertificateNumber = certificate.CertificateNumber,
                    IssueDate = certificate.IssueDate,
                    ExpiryDate = certificate.ExpiryDate,
                    RemainingDays = (certificate.ExpiryDate.Date - today).Days,
                    Status = GetCertificateStatus(certificate.ExpiryDate, today),
                    FileUrl = certificate.FilePath,
                    OriginalFileName = certificate.OriginalFileName,
                    ApprovalStatus = (int)certificate.ApprovalStatus,
                    ApprovalStatusName = GetApprovalStatusName(certificate.ApprovalStatus),
                    ReviewedDate = certificate.ReviewedDate,
                    RejectionReason = certificate.RejectionReason
                }).ToList();
        }

        private static DriverCertificateDto MapCertificate(
            DriverCertificate certificate)
        {
            var today = DateTime.Today;
            return new DriverCertificateDto
            {
                Id = certificate.Id,
                CertificateType = certificate.CertificateType,
                CertificateNumber = certificate.CertificateNumber,
                IssueDate = certificate.IssueDate,
                ExpiryDate = certificate.ExpiryDate,
                RemainingDays = (certificate.ExpiryDate.Date - today).Days,
                Status = GetCertificateStatus(certificate.ExpiryDate, today),
                FileUrl = certificate.FilePath,
                OriginalFileName = certificate.OriginalFileName,
                ApprovalStatus = (int)certificate.ApprovalStatus,
                ApprovalStatusName = GetApprovalStatusName(certificate.ApprovalStatus),
                ReviewedDate = certificate.ReviewedDate,
                RejectionReason = certificate.RejectionReason
            };
        }

        private static string GetCertificateStatus(DateTime expiryDate, DateTime today)
        {
            var remainingDays = (expiryDate.Date - today).Days;
            return expiryDate.Date < today
                ? "Expired"
                : remainingDays <= 30 ? "ExpiringSoon" : "Valid";
        }

        private static string GetApprovalStatusName(
            CertificateApprovalStatusEnum status) => status switch
        {
            CertificateApprovalStatusEnum.Pending => "İnceleniyor",
            CertificateApprovalStatusEnum.Approved => "Onaylandı",
            CertificateApprovalStatusEnum.Rejected => "Reddedildi",
            _ => "Bilinmiyor"
        };

        public async Task<List<DriverPerformanceDto>> GetMyPerformancesAsync(int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new List<DriverPerformanceDto>();
            }

            return await _context.DriverPerformances
                .AsNoTracking()
                .Where(performance => performance.DriverId == driverId.Value)
                .OrderByDescending(performance => performance.EvaluationDate)
                .Select(performance => new DriverPerformanceDto
                {
                    Id = performance.Id,
                    Score = performance.Score,
                    PerformanceComment = performance.PerformanceComment ?? string.Empty,
                    EvaluationDate = performance.EvaluationDate,
                    InspectorFullName = performance.Inspector.User.FirstName
                        + " " + performance.Inspector.User.LastName
                })
                .ToListAsync();
        }

        public async Task<List<DriverTripDto>> GetMyTripsAsync(int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return new List<DriverTripDto>();
            }

            return await _context.Trips
                .AsNoTracking()
                .Where(trip => trip.DriverId == driverId.Value)
                .OrderBy(trip => trip.TripDate)
                .ThenBy(trip => trip.DepertureTime)
                .Select(trip => new DriverTripDto
                {
                    Id = trip.Id,
                    TripDate = trip.TripDate,
                    RouteCode = trip.BusRoute.RouteCode,
                    RouteName = trip.BusRoute.RouteName,
                    VehicleDoorNumber = trip.Vehicle.DoorNumber,
                    DepertureTime = trip.DepertureTime,
                    ArrivalTime = trip.ArrivalTime,
                    TripStatus = trip.TripStatus.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<DriverComplaintDto>?> GetMyComplaintsAsync(
            int userId)
        {
            var driverId = await _context.Drivers
                .AsNoTracking()
                .Where(driver => driver.UserId == userId)
                .Select(driver => (int?)driver.Id)
                .FirstOrDefaultAsync();

            if (driverId is null)
            {
                return null;
            }

            return await _context.Complaints
                .AsNoTracking()
                .Where(complaint =>
                    complaint.TripId.HasValue
                    && complaint.Trip != null
                    && complaint.Trip.DriverId == driverId.Value)
                .OrderByDescending(complaint => complaint.CreatedDate)
                .Select(complaint => new DriverComplaintDto
                {
                    Id = complaint.Id,
                    TrackingCode = complaint.TrackingCode,
                    ComplaintTypeName =
                        complaint.ComplaintType.ComplaintTypeName,
                    ComplaintDescription = complaint.ComplaintDescription,
                    ComplaintCreatedDate = complaint.CreatedDate,
                    ComplaintStatus = complaint.ComplaintStatus,
                    ComplaintStatusName =
                        complaint.ComplaintStatus == ComplaintStatusEnum.Pending
                            ? "Beklemede"
                            : complaint.ComplaintStatus == ComplaintStatusEnum.UnderReview
                                ? "İnceleniyor"
                                : complaint.ComplaintStatus == ComplaintStatusEnum.Resolved
                                    ? "Çözüldü"
                                    : complaint.ComplaintStatus == ComplaintStatusEnum.Rejected
                                        ? "Reddedildi"
                                        : "Bilinmiyor",
                    TripId = complaint.Trip!.Id,
                    TripDate = complaint.Trip!.TripDate,
                    DepertureTime = complaint.Trip.DepertureTime,
                    ArrivalTime = complaint.Trip.ArrivalTime,
                    RouteId = complaint.RouteId,
                    RouteCode = complaint.BusRoute.RouteCode,
                    RouteName = complaint.BusRoute.RouteName,
                    VehicleId = complaint.VehicleId,
                    VehicleDoorNumber = complaint.Vehicle.DoorNumber,
                    StopId = complaint.StopId,
                    StopCode = complaint.BusStop.StopCode,
                    StopName = complaint.BusStop.StopName,
                    InvestigationResult = complaint.Investigations
                        .Where(investigation => investigation.ClosedDate.HasValue)
                        .OrderByDescending(investigation => investigation.ClosedDate)
                        .ThenByDescending(investigation => investigation.Id)
                        .Select(investigation => investigation.InvestigationResult)
                        .FirstOrDefault(),
                    InvestigationClosedDate = complaint.Investigations
                        .Where(investigation => investigation.ClosedDate.HasValue)
                        .OrderByDescending(investigation => investigation.ClosedDate)
                        .ThenByDescending(investigation => investigation.Id)
                        .Select(investigation => investigation.ClosedDate)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        private static string MaskIdentityNumber(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return "***********";
            }

            if (identityNumber.Length < 4)
            {
                return new string('*', identityNumber.Length);
            }

            return identityNumber[..2]
                + new string('*', identityNumber.Length - 4)
                + identityNumber[^2..];
        }
    }
}
