using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class BusStop : IEntity
    {
        public int Id { get; set; }
        
        public string StopCode { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationDescription { get; set; }

        public ICollection<BusRouteStop> BusRouteStops { get; set; } = new List<BusRouteStop>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    }
}