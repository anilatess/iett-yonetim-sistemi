using System.ComponentModel.DataAnnotations;
using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Trips
{
    public class UpdateTripDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Şoför seçimi geçersiz.")]
        public int DriverId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Araç seçimi geçersiz.")]
        public int VehicleId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Hat seçimi geçersiz.")]
        public int RouteId { get; set; }

        public DateTime TripDate { get; set; }
        public TimeSpan DepertureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }

        [EnumDataType(typeof(TripStatusEnum), ErrorMessage = "Sefer durumu geçersiz.")]
        public TripStatusEnum TripStatus { get; set; }
    }
}
