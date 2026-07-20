using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class BusRoute : IEntity
    {
        public int Id { get; set; }

        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
    }
}