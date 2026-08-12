using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Investigations
{
    public class FinalInvestigationDecisionDto
    {
        [Required(ErrorMessage = "Nihai karar zorunludur.")]
        [MaxLength(1000, ErrorMessage = "Nihai karar en fazla 1000 karakter olabilir.")]
        public string Result { get; set; } = string.Empty;
    }
}
