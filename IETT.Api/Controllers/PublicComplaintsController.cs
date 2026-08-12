using IETT.Business.Abstract;
using IETT.Entity.DTOs.Complaints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicComplaintsController : ControllerBase
    {
        private readonly IPublicComplaintService _publicComplaintService;

        public PublicComplaintsController(
            IPublicComplaintService publicComplaintService)
        {
            _publicComplaintService = publicComplaintService;
        }

        [HttpGet("types")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PublicComplaintTypeDto>>> GetTypes()
        {
            var complaintTypes = await _publicComplaintService
                .GetComplaintTypesAsync();

            return Ok(complaintTypes);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<PublicComplaintCreatedDto>> Create(
            CreatePublicComplaintDto dto)
        {
            var result = await _publicComplaintService.CreateAsync(dto);

            if (result.Status != PublicComplaintOperationStatus.Success)
            {
                return BadRequest(new { message = GetErrorMessage(result.Status) });
            }

            return StatusCode(StatusCodes.Status201Created, result.Complaint);
        }

        [HttpGet("{trackingCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicComplaintTrackingDto>> Track(
            string trackingCode)
        {
            var result = await _publicComplaintService
                .GetByTrackingCodeAsync(trackingCode);

            return result.Status switch
            {
                PublicComplaintTrackingLookupStatus.Success => Ok(result.Complaint),
                PublicComplaintTrackingLookupStatus.DuplicateTrackingCode => Conflict(new
                {
                    message = "Takip kodu için veri bütünlüğü sorunu bulundu. Lütfen sistem yöneticisine başvurunuz."
                }),
                _ => NotFound(new { message = "Takip koduna ait şikâyet bulunamadı." })
            };
        }

        private static string GetErrorMessage(
            PublicComplaintOperationStatus status) => status switch
            {
                PublicComplaintOperationStatus.DoorNumberRequired =>
                    "Kapı numarası boş olamaz.",
                PublicComplaintOperationStatus.VehicleNotFound =>
                    "Belirtilen kapı numarasına ait araç bulunamadı.",
                PublicComplaintOperationStatus.VehicleAmbiguous =>
                    "Bu kapı numarasıyla birden fazla araç bulundu. Lütfen sistem yöneticisine başvurunuz.",
                PublicComplaintOperationStatus.RouteNotFound =>
                    "Belirtilen hat koduna ait hat bulunamadı.",
                PublicComplaintOperationStatus.RouteAmbiguous =>
                    "Bu hat koduyla birden fazla hat bulundu. Lütfen sistem yöneticisine başvurunuz.",
                PublicComplaintOperationStatus.TripNotFound =>
                    "Araç ve olay zamanı için uygun bir sefer bulunamadı.",
                PublicComplaintOperationStatus.TripAmbiguous =>
                    "Araç ve olay zamanı için birden fazla uygun sefer bulundu. Lütfen hat kodunu da giriniz.",
                PublicComplaintOperationStatus.InspectorNotAvailable =>
                    "Seferin garajında şikâyetin atanabileceği bir denetimci bulunamadı.",
                PublicComplaintOperationStatus.ComplaintTypeNotFound =>
                    "Geçerli bir şikâyet türü seçiniz.",
                PublicComplaintOperationStatus.IncidentDateTimeRequired =>
                    "Olay tarihi ve saati zorunludur.",
                PublicComplaintOperationStatus.IncidentDateTimeInFuture =>
                    "Olay tarihi ve saati gelecekte olamaz.",
                PublicComplaintOperationStatus.DescriptionRequired =>
                    "Şikâyet açıklaması boş olamaz.",
                PublicComplaintOperationStatus.DescriptionTooLong =>
                    "Şikâyet açıklaması en fazla 2000 karakter olabilir.",
                _ => "Şikâyet oluşturulamadı. Bilgilerinizi kontrol ediniz."
            };
    }
}
