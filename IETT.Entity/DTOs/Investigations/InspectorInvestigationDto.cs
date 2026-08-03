namespace IETT.Entity.DTOs.Investigations
{
    public class InspectorInvestigationDto
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime ComplaintCreatedDate { get; set; }
        public string InvestigationTitle { get; set; } = string.Empty;
        public string InvestigationDescription { get; set; } = string.Empty;
        public string InvestigationResult { get; set; } = string.Empty;
        public DateTime InvestigationCreatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? DriverId { get; set; }
        public string? DriverFullName { get; set; }
        public string? PersonnelNumber { get; set; }
        public int? VehicleId { get; set; }
        public string? VehicleDoorNumber { get; set; }
        public int? RouteId { get; set; }
        public string? RouteCode { get; set; }
        public string? RouteName { get; set; }
        public int? StopId { get; set; }
        public string? StopCode { get; set; }
        public string? StopName { get; set; }
        public int? TripId { get; set; }
        public DateTime? TripDate { get; set; }
        public TimeSpan? DepertureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
    }
}
