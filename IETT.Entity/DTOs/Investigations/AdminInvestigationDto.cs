namespace IETT.Entity.DTOs.Investigations
{
    public class AdminInvestigationDto
    {
        public int InvestigationId { get; set; }
        public int InspectorId { get; set; }
        public string InspectorFullName { get; set; } = string.Empty;
        public int ComplaintId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string ComplaintDescription { get; set; } = string.Empty;
        public int? DriverId { get; set; }
        public string DriverFullName { get; set; } = "Belirtilmedi";
        public string? DriverPersonnelNumber { get; set; }
        public int VehicleId { get; set; }
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public int? RouteId { get; set; }
        public string? RouteCode { get; set; }
        public string? RouteName { get; set; }
        public int? StopId { get; set; }
        public string? StopCode { get; set; }
        public string StopName { get; set; } = "Belirtilmedi";
        public int? TripId { get; set; }
        public DateTime? TripDate { get; set; }
        public TimeSpan? DepertureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string InvestigationTitle { get; set; } = string.Empty;
        public string InvestigationDescription { get; set; } = string.Empty;
        public string? InvestigationResult { get; set; }
        public string Decision { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
