using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Trips
{
    public class InspectorTripListDto
    {
        public int TripId { get; set; }
        public string DriverFullName { get; set; } = string.Empty;
        public string PersonnelNumber { get; set; } = string.Empty;
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public DateTime PlannedDepartureDateTime { get; set; }
        public DateTime PlannedArrivalDateTime { get; set; }
        public TripStatusEnum TripStatus { get; set; }
        public string TripStatusName { get; set; } = string.Empty;
    }
}
