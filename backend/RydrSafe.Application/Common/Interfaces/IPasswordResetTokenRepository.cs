using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);

    /// <summary>Looks a token up by its hash, with the owning user loaded.</summary>
    Task<PasswordResetToken?> GetByHashAsync(string tokenHash);

    /// <summary>
    /// Invalidates every outstanding token for a user. Called when one is redeemed, so a
    /// second reset email sitting in an inbox cannot be used after the first has been spent.
    /// </summary>
    Task InvalidateAllForUserAsync(Guid userId, DateTime usedAt);

    Task SaveChangesAsync();
}
