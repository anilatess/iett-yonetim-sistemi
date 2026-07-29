namespace IETT.Entity.DTOs.BusRouteDtos
{
    public class BusRouteStopDto
    {
        public int StopId { get; set; }

        public string StopCode { get; set; }
            = string.Empty;

        public string StopName { get; set; }
            = string.Empty;

        public int StopOrder { get; set; }

        public string? LocationDescription { get; set; }
    }
}