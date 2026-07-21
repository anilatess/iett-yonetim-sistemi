using IETT.Entity.Interfaces;

namespace IETT.Entity.Entities
{
    public class Driver : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GarageId { get; set; }
        public int OperatorId { get; set; }
        public int DriverStatusId { get; set; }

        public string PersonnelNumber { get; set; } = string.Empty;
        public string HolidayDay { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public Garage Garage { get; set; } = null!;
        public Operator Operator { get; set; } = null!;
        public DriverStatus DriverStatus { get; set; } = null!;
   
        public ICollection<DriverCertificate> DriverCertificates { get; set; } = new List<DriverCertificate>();

        public ICollection<DriverPerformance> DriverPerformances { get; set; } = new List<DriverPerformance>();

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();

    }
}