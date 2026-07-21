using IETT.Entity.Enums;
using IETT.Entity.Interfaces;
namespace IETT.Entity.Entities
{
    public class Complaint : BaseEntity, IEntity
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

        public ComplaintStatusEnum ComplaintStatus { get; set; }
            = ComplaintStatusEnum.Pending;

        public ComplaintType ComplaintType { get; set; } = null!;
        public BusRoute BusRoute { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public BusStop BusStop { get; set; } = null!;
        public Trip? Trip { get; set; }

        public ICollection<Investigation> Investigations { get; set; }
            = new List<Investigation>();
    }
}