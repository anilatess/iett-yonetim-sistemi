namespace IETT.Entity.DTOs.Vehicles
{
    public class CreateVehicleDto
    {
        public string DoorNumber { get; set; } = string.Empty;
        public int VehicleStatusId { get; set; }
    }
}