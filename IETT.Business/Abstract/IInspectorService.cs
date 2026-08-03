using IETT.Entity.DTOs.Performances;
using IETT.Entity.DTOs.Investigations;

namespace IETT.Business.Abstract
{
    public enum InvestigationCompletionStatus
    {
        Completed,
        NotFound,
        AlreadyCompleted
    }

    public interface IInspectorService
    {
        Task<DriverPerformanceDto?> CreatePerformanceAsync(
            int userId,
            CreateDriverPerformanceDto dto
        );

        Task<List<InspectorPerformanceHistoryDto>> GetMyPerformancesAsync(
            int userId
        );

        Task<List<InspectorInvestigationDto>?> GetMyInvestigationsAsync(
            int userId
        );

        Task<InvestigationCompletionStatus> CompleteInvestigationAsync(
            int userId,
            int investigationId,
            CompleteInvestigationDto dto
        );
    }
}
