using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Performances
{
    public class CreateDriverPerformanceDto
    {
        [Range(1, int.MaxValue)]
        public int DriverId { get; set; }

        [Range(0, 100)]
        public int Score { get; set; }

        [MaxLength(500)]
        public string PerformanceComment { get; set; } = string.Empty;
    }
}
