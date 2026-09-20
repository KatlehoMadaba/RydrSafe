using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class RecommendationRepository(AppDbContext db) : IRecommendationRepository
{
    public async Task<Recommendation?> GetByIdAsync(Guid id) =>
        await db.Recommendations.FindAsync(id);

    public async Task<IEnumerable<Recommendation>> GetByUserIdAsync(Guid userId, int page, int pageSize) =>
        await db.Recommendations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public async Task<int> CountByUserIdAsync(Guid userId) =>
        await db.Recommendations.CountAsync(r => r.UserId == userId);

    public async Task AddAsync(Recommendation recommendation)
    {
        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync();
    }
}
