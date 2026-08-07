namespace IETT.Entity.DTOs.Investigations
{
    public class ComplaintForwardedNotificationDto
    {
        public int DriverUserId { get; set; }
        public int ComplaintId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ApprovedDate { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
