using IETT.Entity.Entities;
using IETT.Entity.Interfaces;

namespace IETT.Entity.Entitites
{
    public class Role : IEntity
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}