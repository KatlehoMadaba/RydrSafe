using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class ReportRepository(AppDbContext db) : IReportRepository
{
    public async Task<Report?> GetByIdAsync(Guid id) =>
        await db.Reports.Include(r => r.Driver).Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize) =>
        await db.Reports.Include(r => r.Driver).Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public async Task<int> CountAllAsync() => await db.Reports.CountAsync();

    public async Task<IEnumerable<Report>> GetByDriverIdAsync(Guid driverId) =>
        await db.Reports.Include(r => r.User).Where(r => r.DriverId == driverId).ToListAsync();

    public async Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId) =>
        await db.Reports.Include(r => r.Driver).Where(r => r.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Report>> GetLiveByDriverIdAsync(Guid driverId) =>
        await db.Reports
            .Where(r => r.DriverId == driverId
                        && r.Status != ReportStatus.Rejected
                        && r.Status != ReportStatus.Withdrawn)
            .ToListAsync();

    public async Task<int> CountByDriverIdAsync(Guid driverId) =>
        await db.Reports.CountAsync(r => r.DriverId == driverId);

    /// <summary>
    /// Clause 6.2 — only corroborated reports bear on a driver's public standing. Pending and
    /// approved reports are held privately and deliberately count for nothing here.
    /// </summary>
    public async Task<int> CountCorroboratedByDriverIdAsync(Guid driverId) =>
        await db.Reports.CountAsync(r => r.DriverId == driverId && r.Status == ReportStatus.Corroborated);

    public async Task<int> CountPendingByDriverIdAsync(Guid driverId) =>
        await db.Reports.CountAsync(r => r.DriverId == driverId && r.Status == ReportStatus.Pending);

    /// <summary>
    /// Ordered in memory, not in SQL: severity is persisted as its name, so the database would
    /// sort it alphabetically — "Critical" before "Low" by luck, "High" before "Medium" wrongly.
    /// </summary>
    public async Task<ReportSeverity?> GetHighestPendingSeverityByDriverIdAsync(Guid driverId)
    {
        var severities = await db.Reports
            .Where(r => r.DriverId == driverId && r.Status == ReportStatus.Pending)
            .Select(r => r.Severity)
            .ToListAsync();

        return severities.Count == 0 ? null : severities.Max();
    }

    public async Task<bool> HasCorroboratedPoliceReportAsync(Guid driverId) =>
        await db.Reports.AnyAsync(r =>
            r.DriverId == driverId && r.ReportedToPolice && r.Status == ReportStatus.Corroborated);

    public async Task<IEnumerable<Report>> GetCorroboratedByDriverIdAsync(Guid driverId) =>
        await db.Reports
            .Where(r => r.DriverId == driverId && r.Status == ReportStatus.Corroborated)
            .ToListAsync();

    public async Task AddAsync(Report report)
    {
        db.Reports.Add(report);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Report report)
    {
        db.Reports.Update(report);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// The description stays: it is the substance of the allegation and a driver appealing under
    /// Part C is entitled to have it reviewed. What goes is the link to the person who wrote it.
    /// <c>ReporterKeyHash</c> stays too — it carries no identity on its own, and clause 6.3(a)
    /// needs it to keep telling reporters apart.
    /// </summary>
    public async Task<int> DeIdentifyByUserAsync(Guid userId)
    {
        var reports = await db.Reports.Where(r => r.UserId == userId).ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var report in reports)
        {
            report.UserId = null;
            report.ReporterDeletedAt = now;
        }

        if (reports.Count > 0) await db.SaveChangesAsync();
        return reports.Count;
    }

    public async Task<IEnumerable<Report>> GetExpiredAsync(DateTime cutoff, int batchSize) =>
        await db.Reports
            .Where(r => r.CreatedAt < cutoff)
            .OrderBy(r => r.CreatedAt)
            .Take(batchSize)
            .ToListAsync();

    public async Task DeleteRangeAsync(IEnumerable<Report> reports)
    {
        db.Reports.RemoveRange(reports);
        await db.SaveChangesAsync();
    }
}
