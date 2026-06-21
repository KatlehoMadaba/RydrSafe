using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<Report>> GetByDriverIdAsync(Guid driverId);
    Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId);
    Task<int> CountByDriverIdAsync(Guid driverId);
    Task AddAsync(Report report);
    Task UpdateAsync(Report report);
}
