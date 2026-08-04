using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Trips
{
    public class TripListDto
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string DriverFullName { get; set; } = string.Empty;
        public string PersonnelNumber { get; set; } = string.Empty;
        public int GarageId { get; set; }
        public string GarageName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public int RouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public TimeSpan DepertureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public TripStatusEnum TripStatus { get; set; }
        public string TripStatusName { get; set; } = string.Empty;
    }
}
