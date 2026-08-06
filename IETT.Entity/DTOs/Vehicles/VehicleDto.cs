namespace IETT.Entity.DTOs.Vehicles
{
    public class VehicleDto
    {
        public int Id { get; set; }

        public string DoorNumber { get; set; } = string.Empty;

        public string LicensePlate { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Model { get; set; } = string.Empty;
        public int ProductionYear { get; set; }

        public int VehicleStatusId { get; set; }
    }
}
