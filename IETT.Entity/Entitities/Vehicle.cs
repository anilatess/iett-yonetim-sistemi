using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Vehicle : IEntity
    {
        public int Id { get; set; }

        public string DoorNumber { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;

        public int Capacity { get; set; }
        public int? VehicleStatusId { get; set; }

        public VehicleStatus? VehicleStatus { get; set; }

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}