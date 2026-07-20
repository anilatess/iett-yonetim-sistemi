using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Complaint : IEntity
    {
        public int Id { get; set; }
        public string TrackingCode { get; set; } = string.Empty;

        public int ComplaintTypeId { get; set; }
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public int StopId { get; set; }
        public int? TripId { get; set; }

        public DateTime ComplaintDate { get; set; }
        public TimeSpan ComplaintTime { get; set; }
        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public int? ComplaintStatusId { get; set; }

        public ComplaintType ComplaintType { get; set; } = null!;
        public BusRoute BusRoute { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public BusStop BusStop { get; set; } = null!;
        public Trip? Trip { get; set; }
        public ComplaintStatus? ComplaintStatus { get; set; }
    }
}