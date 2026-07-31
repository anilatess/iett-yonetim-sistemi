namespace IETT.Entity.DTOs.Trips
{
    public class DriverTripDto
    {
        public int Id { get; set; }

        public DateTime TripDate { get; set; }

        public string RouteCode { get; set; } = string.Empty;

        public string RouteName { get; set; } = string.Empty;

        public string VehicleDoorNumber { get; set; } = string.Empty;

        public TimeSpan DepertureTime { get; set; }

        public TimeSpan ArrivalTime { get; set; }

        public string TripStatus { get; set; } = string.Empty;
    }
}