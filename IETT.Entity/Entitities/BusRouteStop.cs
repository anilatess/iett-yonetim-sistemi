using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class BusRouteStop : IEntity
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int StopId { get; set; }
        public int StopOrder { get; set; }

        public BusRoute BusRoute { get; set; } = null!;
        public BusStop BusStop { get; set; } = null!;
    }
}