using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Vehicles
{
    public class CreateVehicleDto : IValidatableObject
    {
        [Required(ErrorMessage = "Kapı numarası zorunludur.")]
        public string DoorNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plaka zorunludur.")]
        [MaxLength(20, ErrorMessage = "Plaka en fazla 20 karakter olabilir.")]
        public string LicensePlate { get; set; } = string.Empty;

        [Range(1, 300, ErrorMessage = "Kapasite 1 ile 300 arasında olmalıdır.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Model zorunludur.")]
        [MaxLength(150, ErrorMessage = "Model en fazla 150 karakter olabilir.")]
        public string Model { get; set; } = string.Empty;

        public int ProductionYear { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Araç durumu seçimi geçersizdir.")]
        public int VehicleStatusId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProductionYear < 1980 || ProductionYear > DateTime.Now.Year + 1)
            {
                yield return new ValidationResult(
                    $"Üretim yılı 1980 ile {DateTime.Now.Year + 1} arasında olmalıdır.",
                    new[] { nameof(ProductionYear) });
            }
        }
    }
}
