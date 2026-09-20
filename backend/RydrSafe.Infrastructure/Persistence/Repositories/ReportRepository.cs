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
