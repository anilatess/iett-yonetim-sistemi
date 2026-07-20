using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class ComplaintType : IEntity
    {
        public int Id { get; set; }
        public string ComplaintTypeName { get; set; } = string.Empty;
    }
}