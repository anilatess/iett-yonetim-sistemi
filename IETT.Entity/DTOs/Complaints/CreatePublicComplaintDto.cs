namespace IETT.Entity.DTOs.Complaints
{
    public class CreatePublicComplaintDto
    {
        public string DoorNumber { get; set; } = string.Empty;
        public string? RouteCode { get; set; }
        public int ComplaintTypeId { get; set; }
        public DateTime IncidentDateTime { get; set; }
        public string ComplaintDescription { get; set; } = string.Empty;
    }
}
