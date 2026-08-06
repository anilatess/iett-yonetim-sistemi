using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.Entity.DTOs.Drivers;
using IETT.Entity.DTOs.Certificates;
using IETT.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly IWebHostEnvironment _environment;

        public DriversController(
            IDriverService driverService,
            IWebHostEnvironment environment)
        {
            _driverService = driverService;
            _environment = environment;
        }

        [HttpGet("me/dashboard")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetDashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var dashboard = await _driverService.GetDashboardAsync(userId);

            return dashboard is null
                ? NotFound(new { message = "Şoför kaydı bulunamadı." })
                : Ok(dashboard);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<ActionResult<List<DriverListDto>>> GetAll()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role is not ("Admin" or "Inspector"))
            {
                return Forbid();
            }

            var drivers = await _driverService
                .GetAllAsync(userId, role);

            if (drivers is null)
            {
                return NotFound(new
                {
                    message = "Denetimci kaydı bulunamadı."
                });
            }

            return Ok(drivers);
        }

        [HttpGet("me/trips")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyTrips()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var trips =
                await _driverService.GetMyTripsAsync(userId);

            return Ok(trips);
        }

        [HttpGet("me/complaints")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyComplaints()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var complaints =
                await _driverService.GetMyComplaintsAsync(userId);

            if (complaints is null)
            {
                return NotFound(new
                {
                    message = "Şoför kaydı bulunamadı."
                });
            }

            return Ok(complaints);
        }

        [HttpGet("me/certificates")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyCertificates()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var certificates =
                await _driverService.GetMyCertificatesAsync(userId);

            return Ok(certificates);
        }

        [HttpPost("me/certificates")]
        [Authorize(Roles = "Driver")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UploadMyCertificate(
            [FromForm] DriverCertificateUploadForm form)
        {
            if (!int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out var userId))
            {
                return Unauthorized();
            }

            var validationError = await ValidateCertificateFormAsync(form);
            if (validationError is not null)
            {
                return validationError;
            }

            var driverId = await _driverService.GetMyDriverIdAsync(userId);
            if (driverId is null)
            {
                return NotFound(new { message = "Şoför kaydı bulunamadı." });
            }

            var extension = Path.GetExtension(form.File!.FileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var webRoot = _environment.WebRootPath
                ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadDirectory = Path.Combine(
                webRoot,
                "uploads",
                "driver-certificates",
                driverId.Value.ToString());
            Directory.CreateDirectory(uploadDirectory);

            var physicalPath = Path.Combine(uploadDirectory, storedFileName);
            var relativePath = $"/uploads/driver-certificates/{driverId.Value}/{storedFileName}";
            var originalFileName = Path.GetFileName(form.File.FileName);
            if (originalFileName.Length > 255)
            {
                originalFileName = originalFileName[..255];
            }

            try
            {
                await using (var stream = new FileStream(
                    physicalPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await form.File.CopyToAsync(stream);
                }

                var result = await _driverService.CreateMyCertificateAsync(
                    userId,
                    new CreateDriverCertificateDto
                    {
                        CertificateType = form.CertificateType,
                        CertificateNumber = form.CertificateNumber,
                        IssueDate = form.IssueDate!.Value,
                        ExpiryDate = form.ExpiryDate!.Value,
                        FilePath = relativePath,
                        OriginalFileName = originalFileName
                    });

                if (result.Status != DriverCertificateCreationStatus.Success)
                {
                    System.IO.File.Delete(physicalPath);
                    return result.Status == DriverCertificateCreationStatus.DriverNotFound
                        ? NotFound(new { message = "Şoför kaydı bulunamadı." })
                        : Conflict(new
                        {
                            message = "Bu sertifika numarası daha önce kaydedilmiş."
                        });
                }

                return Created(relativePath, result.Certificate);
            }
            catch
            {
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                throw;
            }
        }

        private static async Task<IActionResult?> ValidateCertificateFormAsync(
            DriverCertificateUploadForm form)
        {
            if (string.IsNullOrWhiteSpace(form.CertificateType))
                return new BadRequestObjectResult(new { message = "Sertifika türü zorunludur." });
            if (form.CertificateType.Trim().Length > 100)
                return new BadRequestObjectResult(new { message = "Sertifika türü en fazla 100 karakter olabilir." });
            if (string.IsNullOrWhiteSpace(form.CertificateNumber))
                return new BadRequestObjectResult(new { message = "Sertifika numarası zorunludur." });
            if (form.CertificateNumber.Trim().Length > 100)
                return new BadRequestObjectResult(new { message = "Sertifika numarası en fazla 100 karakter olabilir." });
            if (!form.IssueDate.HasValue)
                return new BadRequestObjectResult(new { message = "Düzenlenme tarihi zorunludur." });
            if (!form.ExpiryDate.HasValue)
                return new BadRequestObjectResult(new { message = "Son geçerlilik tarihi zorunludur." });
            if (form.ExpiryDate.Value.Date < form.IssueDate.Value.Date)
                return new BadRequestObjectResult(new { message = "Son geçerlilik tarihi düzenlenme tarihinden önce olamaz." });
            if (form.File is null)
                return new BadRequestObjectResult(new { message = "Sertifika dosyası zorunludur." });
            if (form.File.Length == 0)
                return new BadRequestObjectResult(new { message = "Sertifika dosyası boş olamaz." });
            if (form.File.Length > 5 * 1024 * 1024)
                return new ObjectResult(new { message = "Dosya boyutu en fazla 5 MB olabilir." })
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge
                };

            var extension = Path.GetExtension(form.File.FileName).ToLowerInvariant();
            var allowedTypes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = ["application/pdf"],
                [".jpg"] = ["image/jpeg"],
                [".jpeg"] = ["image/jpeg"],
                [".png"] = ["image/png"]
            };

            if (!allowedTypes.TryGetValue(extension, out var contentTypes)
                || !contentTypes.Contains(form.File.ContentType, StringComparer.OrdinalIgnoreCase)
                || !await HasValidFileSignatureAsync(form.File, extension))
            {
                return new BadRequestObjectResult(new
                {
                    message = "Yalnızca geçerli PDF, JPG, JPEG veya PNG dosyaları yüklenebilir."
                });
            }

            return null;
        }

        private static async Task<bool> HasValidFileSignatureAsync(
            IFormFile file,
            string extension)
        {
            var buffer = new byte[8];
            await using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(buffer);

            return extension switch
            {
                ".pdf" => bytesRead >= 4 && buffer[0] == 0x25 && buffer[1] == 0x50
                    && buffer[2] == 0x44 && buffer[3] == 0x46,
                ".jpg" or ".jpeg" => bytesRead >= 3 && buffer[0] == 0xFF
                    && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => bytesRead >= 8 && buffer.SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                _ => false
            };
        }

        [HttpGet("{driverId:int}/certificates")]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> GetCertificates(int driverId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role is not ("Admin" or "Inspector"))
            {
                return Forbid();
            }

            var certificates = await _driverService.GetCertificatesAsync(
                userId,
                role,
                driverId);

            if (certificates is null)
            {
                return NotFound(new
                {
                    message = "Şoför bulunamadı veya bu şoföre erişim yetkiniz yok."
                });
            }

            return Ok(certificates);
        }

        [HttpGet("me/performances")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyPerformances()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var performances =
                await _driverService.GetMyPerformancesAsync(userId);

            return Ok(performances);
        }

    }
}
