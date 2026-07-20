using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class DriverStatus : IEntity
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    }
} 