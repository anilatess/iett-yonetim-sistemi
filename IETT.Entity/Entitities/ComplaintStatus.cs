using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class ComplaintStatus : IEntity
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}