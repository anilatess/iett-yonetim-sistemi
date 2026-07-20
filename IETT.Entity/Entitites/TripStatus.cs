using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class TripStatus : IEntity
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}