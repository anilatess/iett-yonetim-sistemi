using IETT.Entity.Entitites;
using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Garage : IEntity
    {
        public int Id { get; set; }
        public string GarageName { get; set; } = string.Empty;

        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
        public ICollection<Inspector> Inspectors { get; set; } = new List<Inspector>();
    }
}