namespace IETT.Entity.DTOs.Certificates
{
    public class DriverCertificateDto
    {
        public int Id { get; set; }

        public string? CertificateType { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime? IssueDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int RemainingDays { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? FileUrl { get; set; }

        public string? OriginalFileName { get; set; }

        public int ApprovalStatus { get; set; }

        public string ApprovalStatusName { get; set; } = string.Empty;

        public DateTime? ReviewedDate { get; set; }

        public string? RejectionReason { get; set; }
    }
}
