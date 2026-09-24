using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class ConsentRepository(AppDbContext db) : IConsentRepository
{
    public async Task AddRangeAsync(IEnumerable<UserConsent> consents)
    {
        db.UserConsents.AddRange(consents);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserConsent>> GetByUserIdAsync(Guid userId) =>
        await db.UserConsents
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.AcceptedAt)
            .ToListAsync();

    /// <summary>
    /// A withdrawal marks the existing row rather than deleting it — the record that consent was
    /// once given is itself part of the audit trail.
    /// </summary>
    public async Task WithdrawAsync(Guid userId, string consentKey)
    {
        var rows = await db.UserConsents
            .Where(c => c.UserId == userId && c.ConsentKey == consentKey && c.WithdrawnAt == null)
            .ToListAsync();

        foreach (var row in rows) row.WithdrawnAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task<int> PurgeIpAddressesOlderThanAsync(DateTime cutoff) =>
        await db.UserConsents
            .Where(c => c.AcceptedAt < cutoff && c.IpAddress != null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IpAddress, (string?)null));
}

public class AuditRepository(AppDbContext db) : IAuditRepository
{
    public async Task AddReportAuditAsync(ReportStatusAudit audit)
    {
        db.ReportStatusAudits.Add(audit);
        await db.SaveChangesAsync();
    }

    public async Task AddDriverAuditAsync(DriverStatusAudit audit)
    {
        db.DriverStatusAudits.Add(audit);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReportStatusAudit>> GetReportAuditsAsync(Guid reportId) =>
        await db.ReportStatusAudits
            .Where(a => a.ReportId == reportId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<DriverStatusAudit>> GetDriverAuditsAsync(Guid driverId) =>
        await db.DriverStatusAudits
            .Where(a => a.DriverId == driverId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
}

public class AppealRepository(AppDbContext db) : IAppealRepository
{
    public async Task<DriverAppeal?> GetByIdAsync(Guid id) =>
        await db.DriverAppeals.Include(a => a.Driver).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<DriverAppeal>> GetOpenByDriverIdAsync(Guid driverId) =>
        await db.DriverAppeals
            .Where(a => a.DriverId == driverId
                        && (a.Status == AppealStatus.Received || a.Status == AppealStatus.UnderReview))
            .ToListAsync();

    public async Task<(IEnumerable<DriverAppeal> Items, int Total)> GetPagedAsync(int page, int pageSize)
    {
        var query = db.DriverAppeals.Include(a => a.Driver).OrderByDescending(a => a.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task AddAsync(DriverAppeal appeal)
    {
        db.DriverAppeals.Add(appeal);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(DriverAppeal appeal)
    {
        db.DriverAppeals.Update(appeal);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Compared case-insensitively on both sides — otherwise the abuse limit is sidestepped by
    /// capitalising an address differently on each attempt.
    /// </summary>
    public async Task<int> CountRecentByContactAsync(string contactEmail, DateTime since)
    {
        var normalised = contactEmail.Trim().ToLowerInvariant();

        return await db.DriverAppeals.CountAsync(a =>
            a.ContactEmail.ToLower() == normalised && a.CreatedAt >= since);
    }
}

public class DriverAccessLogRepository(AppDbContext db) : IDriverAccessLogRepository
{
    public async Task AddAsync(DriverRecordAccessLog entry)
    {
        db.DriverRecordAccessLogs.Add(entry);
        await db.SaveChangesAsync();
    }

    public async Task<int> CountByRequesterSinceAsync(string requesterIpHash, DateTime since) =>
        await db.DriverRecordAccessLogs.CountAsync(l =>
            l.RequesterIpHash == requesterIpHash && l.CreatedAt >= since);

    public async Task<int> PurgeOlderThanAsync(DateTime cutoff) =>
        await db.DriverRecordAccessLogs
            .Where(l => l.CreatedAt < cutoff)
            .ExecuteDeleteAsync();
}
