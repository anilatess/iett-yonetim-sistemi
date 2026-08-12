namespace IETT.Entity.DTOs.Complaints
{
    public class PublicComplaintTrackingDto
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? FinalDecision { get; set; }
    }
}
