using IETT.Entity.Entitities;
using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Investigation : BaseEntity, IEntity
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public int InspectorId { get; set; }

        public string InvestigationTitle { get; set; } = string.Empty;
        public string? InvestigationDescription { get; set; }
        public string? InvestigationResult { get; set; }

        public DateTime? ClosedDate { get; set; }

        public Complaint Complaint { get; set; } = null!;
        public Inspector Inspector { get; set; } = null!;
    }
}