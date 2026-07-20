using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class ComplaintStatus : IEntity
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}