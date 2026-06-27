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
            .OrderByDescending(v => v.VerifiedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }
}
