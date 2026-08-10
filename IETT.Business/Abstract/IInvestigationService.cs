using IETT.Entity.DTOs.Investigations;

namespace IETT.Business.Abstract
{
    public interface IInvestigationService
    {
        Task<List<AdminInvestigationDto>> GetAllForAdminAsync();
    }
}
