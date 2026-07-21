using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class DriverCertificate : IEntity
    {
        public int Id { get; set; }
        public int DriverId { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }

        public Driver Driver { get; set; } = null!;
    }
}