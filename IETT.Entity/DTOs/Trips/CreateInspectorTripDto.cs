using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Trips
{
    public class CreateInspectorTripDto
    {
        [Range(1, int.MaxValue)]
        public int DriverId { get; set; }

        [Range(1, int.MaxValue)]
        public int VehicleId { get; set; }

        [Range(1, int.MaxValue)]
        public int RouteId { get; set; }

        public DateTime PlannedDepartureDateTime { get; set; }
        public DateTime PlannedArrivalDateTime { get; set; }
    }
}
