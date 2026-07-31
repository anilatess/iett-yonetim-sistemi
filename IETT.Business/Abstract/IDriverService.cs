using IETT.Entity.DTOs.Certificates;
using IETT.Entity.DTOs.Trips;

namespace IETT.Business.Abstract
{
    public interface IDriverService
    {
        Task<List<DriverCertificateDto>> GetMyCertificatesAsync(int userId);

        Task<List<DriverTripDto>> GetMyTripsAsync(int userId);
    }
}
