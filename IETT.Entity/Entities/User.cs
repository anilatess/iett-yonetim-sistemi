using IETT.Entity.Interfaces;


namespace IETT.Entity.Entities
{
    public class User : BaseEntity, IEntity
    {
        public int Id { get; set; }
        public int RoleId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;


        public Role Role { get; set; } = null!;

        public Driver? Driver { get; set; }
        public Inspector? Inspector { get; set; }
    }
}