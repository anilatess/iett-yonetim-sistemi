using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Investigations;
using IETT.Entity.DTOs.Trips;
using IETT.Entity.DTOs.Certificates;
using IETT.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectorsController : ControllerBase
    {
        private readonly IInspectorService _inspectorService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<InspectorsController> _logger;

        public InspectorsController(
            IInspectorService inspectorService,
            IHubContext<NotificationHub> notificationHub,
            ILogger<InspectorsController> logger)
        {
            _inspectorService = inspectorService;
            _notificationHub = notificationHub;
            _logger = logger;
        }

        [HttpGet("me/dashboard")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetDashboard()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService.GetDashboardAsync(userId);
            return ToGarageScopeResult(result.Status, result.Data);
        }

        [HttpGet("me/certificates")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyCertificates()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });

            var result = await _inspectorService.GetMyCertificatesAsync(userId);
            return ToGarageScopeResult(result.Status, result.Data);
        }

        [HttpPut("me/certificates/{certificateId:int}/approve")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> ApproveCertificate(int certificateId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });

            var result = await _inspectorService.ApproveCertificateAsync(
                userId,
                certificateId);
            return ToCertificateReviewResult(result);
        }

        [HttpPut("me/certificates/{certificateId:int}/reject")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> RejectCertificate(
            int certificateId,
            RejectDriverCertificateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                return BadRequest(new { message = "Ret nedeni zorunludur." });
            if (dto.RejectionReason.Trim().Length > 500)
                return BadRequest(new { message = "Ret nedeni en fazla 500 karakter olabilir." });

            var result = await _inspectorService.RejectCertificateAsync(
                userId,
                certificateId,
                dto.RejectionReason);
            return ToCertificateReviewResult(result);
        }

        private IActionResult ToCertificateReviewResult(
            InspectorCertificateReviewResult result) => result.Status switch
        {
            InspectorCertificateReviewStatus.Success => Ok(result.Certificate),
            InspectorCertificateReviewStatus.InspectorNotFound => NotFound(new
            {
                message = "Denetimci kaydı bulunamadı."
            }),
            InspectorCertificateReviewStatus.GarageNotFound => StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Denetimcinin geçerli bir garaj bağlantısı bulunamadı." }),
            InspectorCertificateReviewStatus.CertificateNotFound => NotFound(new
            {
                message = "Sertifika bulunamadı."
            }),
            InspectorCertificateReviewStatus.OutOfGarageScope => StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Başka bir garaja ait sertifikaya işlem yapılamaz." }),
            InspectorCertificateReviewStatus.AlreadyReviewed => Conflict(new
            {
                message = "Bu sertifika daha önce işlem görmüş."
            }),
            _ => BadRequest(new { message = "Sertifika işlemi tamamlanamadı." })
        };

        [HttpGet("me/drivers")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyDrivers()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService.GetMyDriversAsync(userId);
            return ToGarageScopeResult(result.Status, result.Data);
        }

        [HttpGet("me/trips")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyTrips()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService.GetMyTripsAsync(userId);
            return ToGarageScopeResult(result.Status, result.Data);
        }

        [HttpPost("me/trips")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> CreateMyTrip(CreateInspectorTripDto dto)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService.CreateMyTripAsync(userId, dto);

            if (result.Status == InspectorTripCreationStatus.Success)
            {
                return Created("/api/Inspectors/me/trips", result.Trip);
            }

            return ToTripCreationErrorResult(result.Status);
        }

        [HttpPut("me/trips/{tripId:int}")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> UpdateMyTrip(
            int tripId,
            UpdateInspectorTripDto dto)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService.UpdateMyTripAsync(
                userId,
                tripId,
                dto);

            return result.Status == InspectorTripUpdateStatus.Success
                ? Ok(result.Trip)
                : ToTripUpdateErrorResult(result.Status);
        }

        private IActionResult ToTripUpdateErrorResult(InspectorTripUpdateStatus status)
        {
            return status switch
            {
                InspectorTripUpdateStatus.InspectorNotFound => NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                }),
                InspectorTripUpdateStatus.GarageNotFound => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Denetimcinin geçerli bir garaj bağlantısı bulunamadı." }),
                InspectorTripUpdateStatus.TripNotFound => NotFound(new
                {
                    message = "Sefer bulunamadı."
                }),
                InspectorTripUpdateStatus.TripOutOfGarageScope => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Başka bir garajdaki şoföre ait sefer düzenlenemez." }),
                InspectorTripUpdateStatus.TripNotEditable => Conflict(new
                {
                    message = "Yalnızca planlanmış seferler düzenlenebilir."
                }),
                InspectorTripUpdateStatus.DriverNotFound => NotFound(new
                {
                    message = "Şoför bulunamadı."
                }),
                InspectorTripUpdateStatus.DriverOutOfGarageScope => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Başka bir garaja bağlı şoför seçilemez." }),
                InspectorTripUpdateStatus.DriverUnavailable => Conflict(new
                {
                    message = "Şoförün mevcut durumu görev almaya uygun değil."
                }),
                InspectorTripUpdateStatus.VehicleNotFound => NotFound(new
                {
                    message = "Araç bulunamadı."
                }),
                InspectorTripUpdateStatus.VehicleUnavailable => Conflict(new
                {
                    message = "Aracın mevcut durumu görev almaya uygun değil."
                }),
                InspectorTripUpdateStatus.RouteNotFound => NotFound(new
                {
                    message = "Hat bulunamadı."
                }),
                InspectorTripUpdateStatus.InvalidTimes => BadRequest(new
                {
                    message = "Planlanan kalkış zamanı varış zamanından önce olmalıdır."
                }),
                InspectorTripUpdateStatus.DifferentCalendarDays => BadRequest(new
                {
                    message = "Mevcut sefer yapısı nedeniyle kalkış ve varış aynı takvim gününde olmalıdır."
                }),
                InspectorTripUpdateStatus.TimeInPast => BadRequest(new
                {
                    message = "Planlanan kalkış ve varış zamanları geçmişte olamaz."
                }),
                InspectorTripUpdateStatus.DriverConflict => Conflict(new
                {
                    message = "Şoförün seçilen zaman aralığında çakışan aktif bir seferi var."
                }),
                InspectorTripUpdateStatus.VehicleConflict => Conflict(new
                {
                    message = "Aracın seçilen zaman aralığında çakışan aktif bir seferi var."
                }),
                _ => BadRequest(new { message = "Sefer güncellenemedi." })
            };
        }

        [HttpPut("me/trips/{tripId:int}/cancel")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> CancelMyTrip(int tripId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService.CancelMyTripAsync(userId, tripId);

            return result.Status switch
            {
                InspectorTripCancellationStatus.Success => Ok(result.Trip),
                InspectorTripCancellationStatus.InspectorNotFound => NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                }),
                InspectorTripCancellationStatus.GarageNotFound => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Denetimcinin geçerli bir garaj bağlantısı bulunamadı." }),
                InspectorTripCancellationStatus.TripNotFound => NotFound(new
                {
                    message = "Sefer bulunamadı."
                }),
                InspectorTripCancellationStatus.TripOutOfGarageScope => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Başka bir garajdaki şoföre ait sefer iptal edilemez." }),
                InspectorTripCancellationStatus.AlreadyCancelled => Conflict(new
                {
                    message = "Sefer zaten iptal edilmiş."
                }),
                InspectorTripCancellationStatus.TripNotCancellable => Conflict(new
                {
                    message = "Yalnızca planlanmış seferler iptal edilebilir."
                }),
                _ => BadRequest(new { message = "Sefer iptal edilemedi." })
            };
        }

        private IActionResult ToTripCreationErrorResult(
            InspectorTripCreationStatus status)
        {
            return status switch
            {
                InspectorTripCreationStatus.InspectorNotFound => NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                }),
                InspectorTripCreationStatus.GarageNotFound => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Denetimcinin geçerli bir garaj bağlantısı bulunamadı." }),
                InspectorTripCreationStatus.DriverNotFound => NotFound(new
                {
                    message = "Şoför bulunamadı."
                }),
                InspectorTripCreationStatus.DriverOutOfGarageScope => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Başka bir garaja bağlı şoföre görev atanamaz." }),
                InspectorTripCreationStatus.DriverUnavailable => Conflict(new
                {
                    message = "Şoförün mevcut durumu görev almaya uygun değil."
                }),
                InspectorTripCreationStatus.VehicleNotFound => NotFound(new
                {
                    message = "Araç bulunamadı."
                }),
                InspectorTripCreationStatus.VehicleUnavailable => Conflict(new
                {
                    message = "Aracın mevcut durumu görev almaya uygun değil."
                }),
                InspectorTripCreationStatus.RouteNotFound => NotFound(new
                {
                    message = "Hat bulunamadı."
                }),
                InspectorTripCreationStatus.InvalidTimes => BadRequest(new
                {
                    message = "Planlanan kalkış zamanı varış zamanından önce olmalıdır."
                }),
                InspectorTripCreationStatus.DifferentCalendarDays => BadRequest(new
                {
                    message = "Mevcut sefer yapısı nedeniyle kalkış ve varış aynı takvim gününde olmalıdır."
                }),
                InspectorTripCreationStatus.TimeInPast => BadRequest(new
                {
                    message = "Planlanan kalkış ve varış zamanları geçmişte olamaz."
                }),
                InspectorTripCreationStatus.DriverConflict => Conflict(new
                {
                    message = "Şoförün seçilen zaman aralığında çakışan aktif bir seferi var."
                }),
                InspectorTripCreationStatus.VehicleConflict => Conflict(new
                {
                    message = "Aracın seçilen zaman aralığında çakışan aktif bir seferi var."
                }),
                _ => BadRequest(new { message = "Sefer oluşturulamadı." })
            };
        }

        private bool TryGetUserId(out int userId) => int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);

        private IActionResult ToGarageScopeResult<T>(
            InspectorGarageScopeStatus status,
            T? data)
        {
            return status switch
            {
                InspectorGarageScopeStatus.Success => Ok(data),
                InspectorGarageScopeStatus.InspectorNotFound => NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                }),
                InspectorGarageScopeStatus.GarageNotFound => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Denetimcinin geçerli bir garaj bağlantısı bulunamadı." }),
                _ => Forbid()
            };
        }

        [HttpGet("me/investigations")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyInvestigations()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var investigations = await _inspectorService
                .GetMyInvestigationsAsync(userId);

            if (investigations is null)
            {
                return NotFound("Denetimci kaydı bulunamadı.");
            }

            return Ok(investigations);
        }

        [HttpPut("me/investigations/{id:int}/complete")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> CompleteInvestigation(
            int id,
            CompleteInvestigationDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var completionResult = await _inspectorService
                .CompleteInvestigationAsync(userId, id, dto);

            await SendComplaintForwardedNotificationAsync(completionResult);

            return completionResult.Status switch
            {
                InvestigationCompletionStatus.Completed => NoContent(),
                InvestigationCompletionStatus.AlreadyCompleted => Conflict(new
                {
                    message = "Bu inceleme görevi daha önce sonuçlandırılmış."
                }),
                InvestigationCompletionStatus.InspectorNotFound => NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                }),
                InvestigationCompletionStatus.NotAssigned => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "Bu inceleme görevi giriş yapan denetimciye atanmamış." }),
                InvestigationCompletionStatus.MissingTrip => Conflict(new
                {
                    message = "Şikâyet bir sefere bağlı olmadığı için şoföre iletilemedi."
                }),
                InvestigationCompletionStatus.MissingDriver => Conflict(new
                {
                    message = "Şikâyetin bağlı olduğu sefere atanmış şoför bulunamadı."
                }),
                _ => NotFound(new
                {
                    message = "İnceleme görevi bulunamadı."
                })
            };
        }

        [HttpPut("me/investigations/{id:int}/decision")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> DecideInvestigation(
            int id,
            InvestigationDecisionDto dto)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            var result = await _inspectorService
                .DecideInvestigationAsync(userId, id, dto);

            await SendComplaintForwardedNotificationAsync(result);

            return result.Status switch
            {
                InvestigationCompletionStatus.Completed => NoContent(),
                InvestigationCompletionStatus.InspectorNotFound => NotFound(new { message = "Denetimci kaydı bulunamadı." }),
                InvestigationCompletionStatus.InvestigationNotFound => NotFound(new { message = "İnceleme görevi bulunamadı." }),
                InvestigationCompletionStatus.NotAssigned => StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Bu inceleme görevi giriş yapan denetimciye atanmamış." }),
                InvestigationCompletionStatus.AlreadyCompleted => Conflict(new { message = "Bu inceleme için daha önce karar verilmiş." }),
                InvestigationCompletionStatus.MissingTrip => Conflict(new { message = "Şikâyet bir sefere bağlı olmadığı için onaylanamaz." }),
                InvestigationCompletionStatus.MissingDriver => Conflict(new { message = "Şikâyetin bağlı olduğu sefere atanmış şoför bulunamadı." }),
                _ => BadRequest(new { message = "Karar işlemi tamamlanamadı." })
            };
        }

        private async Task SendComplaintForwardedNotificationAsync(
            InvestigationDecisionResult result)
        {
            if (result.Status != InvestigationCompletionStatus.Completed
                || result.Notification is null)
            {
                return;
            }

            try
            {
                var notification = result.Notification;
                await _notificationHub.Clients
                    .Group(NotificationGroupNames.ForDriverUser(notification.DriverUserId))
                    .SendAsync("ComplaintForwarded", new
                    {
                        notification.ComplaintId,
                        notification.TrackingCode,
                        notification.ComplaintTypeName,
                        notification.Message,
                        notification.ApprovedDate,
                        notification.StatusName
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "ComplaintForwarded bildirimi gönderilemedi. ComplaintId: {ComplaintId}",
                    result.Notification.ComplaintId);
            }
        }

        [HttpGet("me/performances")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> GetMyPerformances()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var performances = await _inspectorService
                .GetMyPerformancesAsync(userId);

            return Ok(performances);
        }

        [HttpPost("me/performances")]
        [Authorize(Roles = "Inspector")]
        public async Task<IActionResult> CreatePerformance(
            CreateDriverPerformanceDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var performance = await _inspectorService
                .CreatePerformanceAsync(userId, dto);

            if (performance is null)
            {
                return NotFound("Denetimci veya şoför kaydı bulunamadı.");
            }

            return Ok(performance);
        }
    }
}
