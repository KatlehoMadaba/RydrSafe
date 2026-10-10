using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository(AppDbContext db) : IPasswordResetTokenRepository
{
    public async Task AddAsync(PasswordResetToken token) =>
        await db.PasswordResetTokens.AddAsync(token);

    public async Task<PasswordResetToken?> GetByHashAsync(string tokenHash) =>
        await db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task InvalidateAllForUserAsync(Guid userId, DateTime usedAt) =>
        await db.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, usedAt));

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
