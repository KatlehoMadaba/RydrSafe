using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class DriverFollowRepository(AppDbContext db) : IDriverFollowRepository
{
    public async Task<bool> IsFollowingAsync(Guid userId, Guid driverId) =>
        await db.DriverFollows.AnyAsync(f => f.UserId == userId && f.DriverId == driverId);

    public async Task FollowAsync(DriverFollow follow)
    {
        db.DriverFollows.Add(follow);
        await db.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid userId, Guid driverId)
    {
        var follow = await db.DriverFollows.FirstOrDefaultAsync(f => f.UserId == userId && f.DriverId == driverId);
        if (follow is not null)
        {
            db.DriverFollows.Remove(follow);
            await db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<DriverFollow>> GetFollowersByDriverIdAsync(Guid driverId) =>
        await db.DriverFollows.Where(f => f.DriverId == driverId).ToListAsync();

    public async Task<IEnumerable<DriverFollow>> GetFollowedDriversByUserIdAsync(Guid userId) =>
        await db.DriverFollows
            .Include(f => f.Driver).ThenInclude(d => d.Vehicles)
            .Where(f => f.UserId == userId)
            .ToListAsync();
}
