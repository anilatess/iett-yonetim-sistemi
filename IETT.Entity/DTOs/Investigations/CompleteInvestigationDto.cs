using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Investigations
{
    public class CompleteInvestigationDto
    {
        [Required(ErrorMessage = "İnceleme sonucu zorunludur.")]
        [MaxLength(1000, ErrorMessage = "İnceleme sonucu en fazla 1000 karakter olabilir.")]
        public string InvestigationResult { get; set; } = string.Empty;
    }
}
