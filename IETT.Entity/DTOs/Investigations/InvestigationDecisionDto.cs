using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Investigations
{
    public class InvestigationDecisionDto
    {
        [Required(ErrorMessage = "Karar zorunludur.")]
        [RegularExpression("^(Approved|Rejected)$",
            ErrorMessage = "Karar yalnızca Approved veya Rejected olabilir.")]
        public string Decision { get; set; } = string.Empty;

        [Required(ErrorMessage = "İnceleme sonucu zorunludur.")]
        [MaxLength(1000, ErrorMessage = "İnceleme sonucu en fazla 1000 karakter olabilir.")]
        public string Result { get; set; } = string.Empty;
    }
}
