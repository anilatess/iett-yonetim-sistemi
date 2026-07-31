using IETT.Entity.DTOs.Trips;

namespace IETT.Business.Abstract
{
    public interface IDriverService
    {
        Task<List<DriverTripDto>> GetMyTripsAsync(int userId);
    }
}
