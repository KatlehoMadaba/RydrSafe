using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Passenger;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Clause 3.1. Self-declared date of birth, captured at registration so the 18+ rule can be
    /// applied and re-checked. This is a product rule, not a POPIA requirement — see
    /// COMPLIANCE-NOTES B10.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>The version of the user agreement this account most recently accepted.</summary>
    public string? AcceptedAgreementVersion { get; set; }
    public DateTime? AcceptedAgreementAt { get; set; }

    /// <summary>Set when clause 3.2 closure applies (account holder established to be under 18).</summary>
    public DateTime? ClosedAt { get; set; }
    public string? ClosureReason { get; set; }

    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<UserConsent> Consents { get; set; } = [];
}
