using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class VehicleStatus : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Vehicle> Vehicles { get; set; }
            = new List<Vehicle>();
    }
}