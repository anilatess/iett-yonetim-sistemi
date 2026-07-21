using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Trip : IEntity
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }

        public DateTime TripDate { get; set; }
        public TimeSpan DepertureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }

        public int? TripStatusId { get; set; }

        public BusRoute BusRoute { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Driver Driver { get; set; } = null!;
        public TripStatus? TripStatus { get; set; }

        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}