using IETT.Entity.DTOs.Performances;

namespace IETT.Business.Abstract
{
    public interface IInspectorService
    {
        Task<DriverPerformanceDto?> CreatePerformanceAsync(
            int userId,
            CreateDriverPerformanceDto dto
        );

        Task<List<InspectorPerformanceHistoryDto>> GetMyPerformancesAsync(
            int userId
        );
    }
}
