using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class VerificationHistoryRepository(AppDbContext db) : IVerificationHistoryRepository
{
    public async Task AddAsync(VerificationHistory entry)
    {
        db.VerificationHistories.Add(entry);
        await db.SaveChangesAsync();
    }

    public async Task<(IEnumerable<VerificationHistory> Items, int Total)> GetByUserIdAsync(Guid userId, int page, int pageSize)
    {
        var query = db.VerificationHistories
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.VerifiedAt)
            .ThenBy(v => v.Id);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }

    public async Task<(int Total, int Flagged, int Safe)> GetStatsByUserIdAsync(Guid userId)
    {
        var stats = await db.VerificationHistories
            .Where(v => v.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Flagged = g.Count(v => v.Status == "Flagged" || v.Status == "HighRisk"),
                Safe = g.Count(v => v.Status == "Safe"),
            })
            .FirstOrDefaultAsync();

        return (stats?.Total ?? 0, stats?.Flagged ?? 0, stats?.Safe ?? 0);
    }

    /// <summary>
    /// Clause 25 / 29. Strips the OCR-derived fields once the retention period is up. The row
    /// survives so the user's own history still shows that a verification happened on that date,
    /// but the driver's name, registration and image hashes are gone.
    /// </summary>
    public async Task<int> PurgeExpiredOcrDataAsync(DateTime cutoff, int batchSize)
    {
        var expired = await db.VerificationHistories
            .Where(v => v.OcrDataPurgedAt == null
                        && v.OcrDataRetainedUntil != null
                        && v.OcrDataRetainedUntil < cutoff)
            .OrderBy(v => v.OcrDataRetainedUntil)
            .Take(batchSize)
            .ToListAsync();

        if (expired.Count == 0) return 0;

        foreach (var row in expired)
        {
            row.DriverName = null;
            row.RegistrationNumber = null;
            row.ImageHashes = null;
            row.OcrDataPurgedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return expired.Count;
    }
}
