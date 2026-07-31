namespace IETT.Entity.DTOs.Performances
{
    public class InspectorPerformanceHistoryDto
    {
        public int Id { get; set; }

        public int DriverId { get; set; }

        public string DriverFullName { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        public int Score { get; set; }

        public string PerformanceComment { get; set; } = string.Empty;

        public DateTime EvaluationDate { get; set; }
    }
}
