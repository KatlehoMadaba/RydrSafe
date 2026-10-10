namespace RydrSafe.Domain.Entities;

/// <summary>
/// A one-time password reset grant.
/// <para>
/// Only the <em>hash</em> of the token is stored. The clear token exists in the email we send
/// and nowhere else, so a leaked database hands an attacker nothing they can redeem — the same
/// reasoning that applies to <see cref="User.PasswordHash"/>.
/// </para>
/// </summary>
public class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    /// <summary>SHA-256 of the token sent by email. Never the token itself.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set the moment the token is redeemed, so it cannot be used twice.</summary>
    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public bool IsRedeemable(DateTime now) => UsedAt is null && ExpiresAt > now;
}
