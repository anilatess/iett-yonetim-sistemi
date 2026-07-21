using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class BusRoute : IEntity
    {
        public int Id { get; set; }

        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
        public ICollection<BusRouteStop> BusRouteStops { get; set; } = new List<BusRouteStop>();

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>(); 

    }
}