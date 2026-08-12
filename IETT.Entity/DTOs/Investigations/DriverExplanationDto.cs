using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Investigations
{
    public class DriverExplanationDto
    {
        [Required(ErrorMessage = "Şoför açıklaması zorunludur.")]
        [MaxLength(1000, ErrorMessage = "Şoför açıklaması en fazla 1000 karakter olabilir.")]
        public string Explanation { get; set; } = string.Empty;
    }
}
