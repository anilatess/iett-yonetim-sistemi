namespace IETT.Entity.DTOs.Certificates
{
    public class InspectorCertificateDto
    {
        public int CertificateId { get; set; }
        public int DriverId { get; set; }
        public string DriverFullName { get; set; } = string.Empty;
        public string PersonnelNumber { get; set; } = string.Empty;
        public string? CertificateType { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int ApprovalStatus { get; set; }
        public string ApprovalStatusName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? FileUrl { get; set; }
        public string? OriginalFileName { get; set; }
        public int RemainingDays { get; set; }
    }

    public class RejectDriverCertificateDto
    {
        public string RejectionReason { get; set; } = string.Empty;
    }
}
