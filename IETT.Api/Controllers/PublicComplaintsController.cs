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

        private static string GetErrorMessage(
            PublicComplaintOperationStatus status) => status switch
            {
                PublicComplaintOperationStatus.ComplaintTypeNotFound =>
                    "Geçerli bir şikâyet türü seçiniz.",
                PublicComplaintOperationStatus.RouteNotFound =>
                    "Geçerli bir hat seçiniz.",
                PublicComplaintOperationStatus.VehicleNotFound =>
                    "Geçerli bir araç seçiniz.",
                PublicComplaintOperationStatus.StopNotFound =>
                    "Geçerli bir durak seçiniz.",
                PublicComplaintOperationStatus.TripNotFound =>
                    "Belirtilen sefer bulunamadı.",
                PublicComplaintOperationStatus.TripRouteMismatch =>
                    "Seçilen hat, belirtilen seferin hattıyla uyuşmuyor.",
                PublicComplaintOperationStatus.TripVehicleMismatch =>
                    "Seçilen araç, belirtilen seferin aracıyla uyuşmuyor.",
                PublicComplaintOperationStatus.ComplaintDateRequired =>
                    "Şikâyet tarihi zorunludur.",
                PublicComplaintOperationStatus.ComplaintTimeRequired =>
                    "Şikâyet saati zorunludur.",
                PublicComplaintOperationStatus.DescriptionRequired =>
                    "Şikâyet açıklaması boş olamaz.",
                PublicComplaintOperationStatus.DescriptionTooLong =>
                    "Şikâyet açıklaması en fazla 2000 karakter olabilir.",
                _ => "Şikâyet oluşturulamadı. Bilgilerinizi kontrol ediniz."
            };
    }
}
