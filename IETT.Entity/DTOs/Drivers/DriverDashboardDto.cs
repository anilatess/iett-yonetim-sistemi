using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Drivers
{
    public class DriverDashboardDto
    {
        public DriverDashboardInfoDto Driver { get; set; } = new();
        public List<DriverDashboardTripDto> TodayTrips { get; set; } = new();
        public List<DriverDashboardComplaintDto> Complaints { get; set; } = new();
    }

    public class DriverDashboardInfoDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PersonnelNumber { get; set; } = string.Empty;
        public string GarageName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string DriverStatusName { get; set; } = string.Empty;
        public string HolidayDay { get; set; } = string.Empty;
    }

    public class DriverDashboardTripDto
    {
        public int TripId { get; set; }
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public DateTime PlannedDepartureDateTime { get; set; }
        public DateTime PlannedArrivalDateTime { get; set; }
        public TripStatusEnum TripStatus { get; set; }
        public string TripStatusName { get; set; } = string.Empty;
    }

    public class DriverDashboardComplaintDto
    {
        public int ComplaintId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string ComplaintStatusName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public int TripId { get; set; }
    }
}
