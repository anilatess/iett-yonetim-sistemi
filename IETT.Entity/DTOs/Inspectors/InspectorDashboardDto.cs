using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Inspectors
{
    public class InspectorDashboardDto
    {
        public InspectorDashboardGarageDto Garage { get; set; } = new();
        public List<InspectorDashboardTripDto> ActiveTrips { get; set; } = new();
        public List<InspectorDashboardTripDto> CancelledTripsToday { get; set; } = new();
        public List<InspectorDashboardComplaintDto> ComplaintsToday { get; set; } = new();
    }

    public class InspectorDashboardGarageDto
    {
        public int GarageId { get; set; }
        public string GarageName { get; set; } = string.Empty;
        public int TotalDriverCount { get; set; }
    }

    public class InspectorDashboardTripDto
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

    public class InspectorDashboardComplaintDto
    {
        public int ComplaintId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? DriverFullName { get; set; }
        public string? PersonnelNumber { get; set; }
        public string? VehicleDoorNumber { get; set; }
        public string? RouteCode { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
