using System.ComponentModel.DataAnnotations;

namespace IETT.Entity.DTOs.Vehicles
{
    public class UpdateVehicleStatusDto
    {
        [Range(1, int.MaxValue)]
        public int VehicleStatusId { get; set; }
    }
}
