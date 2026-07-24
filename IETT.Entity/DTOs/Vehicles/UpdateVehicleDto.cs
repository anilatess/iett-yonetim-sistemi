namespace IETT.Entity.DTOs.Vehicles
{
    public class UpdateVehicleDto
    {
        public int Id { get; set; }
        public string DoorNumber { get; set; } = string.Empty;
        public int VehicleStatusId { get; set; }
    }
}