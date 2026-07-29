namespace IETT.Entity.DTOs.BusRouteDtos
{
    public class BusRouteDto
    {
        public int Id { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
    }
}