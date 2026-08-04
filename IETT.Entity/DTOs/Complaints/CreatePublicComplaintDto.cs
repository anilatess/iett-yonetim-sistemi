namespace IETT.Entity.DTOs.Complaints
{
    public class CreatePublicComplaintDto
    {
        public int ComplaintTypeId { get; set; }
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public int StopId { get; set; }
        public int? TripId { get; set; }
        public DateTime? ComplaintDate { get; set; }
        public TimeSpan? ComplaintTime { get; set; }
        public string ComplaintDescription { get; set; } = string.Empty;
    }
}
