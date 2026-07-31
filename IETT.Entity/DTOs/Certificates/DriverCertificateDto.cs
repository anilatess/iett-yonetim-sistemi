namespace IETT.Entity.DTOs.Certificates
{
    public class DriverCertificateDto
    {
        public int Id { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public int RemainingDays { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
