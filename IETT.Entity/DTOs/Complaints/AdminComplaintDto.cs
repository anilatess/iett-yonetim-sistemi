using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Complaints
{
    public class AdminComplaintDto
    {
        public int Id { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime ComplaintDate { get; set; }
        public TimeSpan ComplaintTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public ComplaintStatusEnum ComplaintStatus { get; set; }
        public string ComplaintStatusName { get; set; } = string.Empty;
        public int? RouteId { get; set; }
        public string? RouteCode { get; set; }
        public string? RouteName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public int? StopId { get; set; }
        public string StopCode { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public int? TripId { get; set; }
        public DateTime? TripDate { get; set; }
        public TimeSpan? DepertureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public int? DriverId { get; set; }
        public string? DriverFullName { get; set; }
        public string? DriverPersonnelNumber { get; set; }
        public int? InvestigationId { get; set; }
        public string? InvestigationTitle { get; set; }
        public string? InvestigationDescription { get; set; }
        public string? InvestigationResult { get; set; }
        public DateTime? InvestigationCreatedDate { get; set; }
        public DateTime? InvestigationClosedDate { get; set; }
        public int? InspectorId { get; set; }
        public string? InspectorFullName { get; set; }
    }
}
