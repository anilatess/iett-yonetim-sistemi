using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class DriverPerformance : IEntity
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public int InspectorId { get; set; }

        public int Score { get; set; }
        public string? PerformanceComment { get; set; }
        public DateTime DateTime { get; set; }

        public Driver Driver { get; set; } = null!;
        public Inspector Inspector { get; set; } = null!;

    }
}