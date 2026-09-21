using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

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

    /// <summary>
    /// Reports filed against this driver that no moderator has looked at yet.
    ///
    /// Disclosed at verification so a passenger is not told "Safe" about a driver with unread
    /// reports against them. It is deliberately <c>Pending</c> only: an <c>Approved</c> report
    /// has been read and is being held back by the clause 6.3 threshold, and surfacing those
    /// would route around the threshold itself.
    /// </summary>
    Task<int> CountPendingByDriverIdAsync(Guid driverId);

    /// <summary>
    /// The highest severity among this driver's unreviewed reports, or null where there are none.
    ///
    /// This is the severity the <em>reporter</em> selected. Nobody has assessed it, so anything
    /// showing it must say so — it is one person's characterisation of their own complaint, not
    /// a finding by RydrSafe.
    /// </summary>
    Task<ReportSeverity?> GetHighestPendingSeverityByDriverIdAsync(Guid driverId);

    /// <summary>Corroborated reports where the reporter indicated the incident went to the police.</summary>
    Task<bool> HasCorroboratedPoliceReportAsync(Guid driverId);

    /// <summary>Clause 6.4 — the reduced public view: counts, bands and a date range, nothing else.</summary>
    Task<IEnumerable<Report>> GetCorroboratedByDriverIdAsync(Guid driverId);

    Task AddAsync(Report report);
    Task UpdateAsync(Report report);

    /// <summary>
    /// Clause 29.2 — unlink every report this account submitted, keeping the reports themselves.
    /// They concern drivers who may still be disputing them, so they survive the account.
    /// Returns how many rows were de-identified, so the caller can tell the user.
    /// </summary>
    Task<int> DeIdentifyByUserAsync(Guid userId);

    /// <summary>Clause 29 — reports past their retention date, for the purge worker.</summary>
    Task<IEnumerable<Report>> GetExpiredAsync(DateTime cutoff, int batchSize);
    Task DeleteRangeAsync(IEnumerable<Report> reports);
}
