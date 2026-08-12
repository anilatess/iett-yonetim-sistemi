using IETT.Entity.Enums;

namespace IETT.Entity.DTOs.Complaints
{
    public class DriverComplaintDto
    {
        public int Id { get; set; }
        public int ComplaintId => Id;
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime ComplaintCreatedDate { get; set; }
        public DateTime ComplaintDate { get; set; }
        public TimeSpan ComplaintTime { get; set; }
        public ComplaintStatusEnum ComplaintStatus { get; set; }
        public string ComplaintStatusName { get; set; } = string.Empty;
        public string StatusName => ComplaintStatusName;
        public int TripId { get; set; }
        public DateTime TripDate { get; set; }
        public TimeSpan DepertureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public int? RouteId { get; set; }
        public string? RouteCode { get; set; }
        public string? RouteName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleDoorNumber { get; set; } = string.Empty;
        public int? StopId { get; set; }
        public string StopCode { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string? InvestigationResult { get; set; }
        public string? InspectorResult => InvestigationResult;
        public DateTime? InvestigationClosedDate { get; set; }
        public DateTime? ApprovedDate => InvestigationClosedDate;
        public int InvestigationId { get; set; }
        public string? DriverExplanation { get; set; }
        public DateTime? DriverExplanationDate { get; set; }
        public InvestigationProcessStatus ProcessStatus { get; set; }
        public string ProcessStatusName { get; set; } = string.Empty;
    }
}
