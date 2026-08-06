using Microsoft.AspNetCore.Http;

namespace IETT.Api.Models
{
    public class DriverCertificateUploadForm
    {
        public string CertificateType { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public IFormFile? File { get; set; }
    }
}
