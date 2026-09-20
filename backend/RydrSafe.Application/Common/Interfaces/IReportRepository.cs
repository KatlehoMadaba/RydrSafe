using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize);
    Task<int> CountAllAsync();
    Task<IEnumerable<Report>> GetByDriverIdAsync(Guid driverId);
    Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId);

    /// <summary>Every live report naming this driver, used for the clause 6.3(a) independence test.</summary>
    Task<IEnumerable<Report>> GetLiveByDriverIdAsync(Guid driverId);

    Task<int> CountByDriverIdAsync(Guid driverId);

    /// <summary>
    /// Reports that count towards the driver's public standing. Clause 6.2: only
    /// <c>Corroborated</c> reports do.
    /// </summary>
    Task<int> CountCorroboratedByDriverIdAsync(Guid driverId);

    /// <summary>Corroborated reports where the reporter indicated the incident went to the police.</summary>
    Task<bool> HasCorroboratedPoliceReportAsync(Guid driverId);

    /// <summary>Clause 6.4 — the reduced public view: counts, bands and a date range, nothing else.</summary>
    Task<IEnumerable<Report>> GetCorroboratedByDriverIdAsync(Guid driverId);

    Task AddAsync(Report report);
    Task UpdateAsync(Report report);

    /// <summary>Clause 29 — reports past their retention date, for the purge worker.</summary>
    Task<IEnumerable<Report>> GetExpiredAsync(DateTime cutoff, int batchSize);
    Task DeleteRangeAsync(IEnumerable<Report> reports);
}
