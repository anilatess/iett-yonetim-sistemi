using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Operator : IEntity
    {
        public int Id { get; set; }
        public string OperatorName { get; set; } = string.Empty;

        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    }
}