using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

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

    public async Task<IEnumerable<Report>> GetByDriverIdAsync(Guid driverId) =>
        await db.Reports.Include(r => r.User).Where(r => r.DriverId == driverId).ToListAsync();

    public async Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId) =>
        await db.Reports.Include(r => r.Driver).Where(r => r.UserId == userId).ToListAsync();

    public async Task<int> CountByDriverIdAsync(Guid driverId) =>
        await db.Reports.CountAsync(r => r.DriverId == driverId);

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
}
