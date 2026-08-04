namespace IETT.Entity.DTOs.Complaints
{
    public class PublicComplaintCreatedDto
    {
        public int Id { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ComplaintStatusName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
