using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IAppealRepository
{
    Task<DriverAppeal?> GetByIdAsync(Guid id);
    Task<IEnumerable<DriverAppeal>> GetOpenByDriverIdAsync(Guid driverId);
    Task<(IEnumerable<DriverAppeal> Items, int Total)> GetPagedAsync(int page, int pageSize);
    Task AddAsync(DriverAppeal appeal);
    Task UpdateAsync(DriverAppeal appeal);
    Task<int> CountRecentByContactAsync(string contactEmail, DateTime since);
}
