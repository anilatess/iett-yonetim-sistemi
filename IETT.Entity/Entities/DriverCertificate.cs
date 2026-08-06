using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class DriverCertificate : IEntity
    {
        public int Id { get; set; }
        public int DriverId { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;
        public string? CertificateType { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? FilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public IETT.Entity.Enums.CertificateApprovalStatusEnum ApprovalStatus { get; set; }
            = IETT.Entity.Enums.CertificateApprovalStatusEnum.Pending;
        public int? ReviewedByInspectorId { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? RejectionReason { get; set; }

        public Driver Driver { get; set; } = null!;
        public Inspector? ReviewedByInspector { get; set; }
    }
}
