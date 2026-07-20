using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Inspector : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GarageId { get; set; }

        public User User { get; set; } = null!; 
        public Garage Garage { get; set; } = null!;
    }
}