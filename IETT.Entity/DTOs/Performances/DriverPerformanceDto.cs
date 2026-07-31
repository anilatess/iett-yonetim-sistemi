namespace IETT.Entity.DTOs.Performances
{
    public class DriverPerformanceDto
    {
        public int Id { get; set; }

        public int Score { get; set; }

        public string PerformanceComment { get; set; } = string.Empty;

        public DateTime EvaluationDate { get; set; }

        public string InspectorFullName { get; set; } = string.Empty;
    }
}
