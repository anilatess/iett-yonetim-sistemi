using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Performances;
using IETT.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace IETT.Business.Concrete
{
    public class InspectorManager : IInspectorService
    {
        private readonly IETTDbContext _context;

        public InspectorManager(IETTDbContext context)
        {
            _context = context;
        }

        public async Task<DriverPerformanceDto?> CreatePerformanceAsync(
            int userId,
            CreateDriverPerformanceDto dto)
        {
            var inspector = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => new
                {
                    inspector.Id,
                    inspector.User.FirstName,
                    inspector.User.LastName
                })
                .FirstOrDefaultAsync();

            if (inspector is null)
            {
                return null;
            }

            var driverExists = await _context.Drivers
                .AsNoTracking()
                .AnyAsync(driver => driver.Id == dto.DriverId);

            if (!driverExists)
            {
                return null;
            }

            var performance = new DriverPerformance
            {
                DriverId = dto.DriverId,
                InspectorId = inspector.Id,
                Score = dto.Score,
                PerformanceComment = dto.PerformanceComment ?? string.Empty,
                EvaluationDate = DateTime.Now
            };

            _context.DriverPerformances.Add(performance);
            await _context.SaveChangesAsync();

            return new DriverPerformanceDto
            {
                Id = performance.Id,
                Score = performance.Score,
                PerformanceComment = performance.PerformanceComment ?? string.Empty,
                EvaluationDate = performance.EvaluationDate,
                InspectorFullName = inspector.FirstName + " " + inspector.LastName
            };
        }

        public async Task<List<InspectorPerformanceHistoryDto>> GetMyPerformancesAsync(
            int userId)
        {
            var inspectorId = await _context.Inspectors
                .AsNoTracking()
                .Where(inspector => inspector.UserId == userId)
                .Select(inspector => (int?)inspector.Id)
                .FirstOrDefaultAsync();

            if (inspectorId is null)
            {
                return new List<InspectorPerformanceHistoryDto>();
            }

            return await _context.DriverPerformances
                .AsNoTracking()
                .Where(performance => performance.InspectorId == inspectorId.Value)
                .OrderByDescending(performance => performance.EvaluationDate)
                .Select(performance => new InspectorPerformanceHistoryDto
                {
                    Id = performance.Id,
                    DriverId = performance.DriverId,
                    DriverFullName = performance.Driver.User.FirstName
                        + " " + performance.Driver.User.LastName,
                    PersonnelNumber = performance.Driver.PersonnelNumber,
                    Score = performance.Score,
                    PerformanceComment = performance.PerformanceComment ?? string.Empty,
                    EvaluationDate = performance.EvaluationDate
                })
                .ToListAsync();
        }
    }
}
