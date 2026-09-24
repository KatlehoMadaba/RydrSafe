using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

/// <summary>
/// Part C clause 35. A driver's route to dispute what RydrSafe holds about them.
/// Lodging an appeal suspends the driver's public status (clause 35.4) until it is resolved.
/// </summary>
public class DriverAppeal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DriverId { get; set; }

    /// <summary>Grounds relied on: inaccuracy, mistaken identity, duplicate profile, or objection to processing.</summary>
    public string Grounds { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;

    /// <summary>Contact detail supplied by the appellant so we can respond. POPIA s23 request channel.</summary>
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }

    /// <summary>What the appellant offered to show they are the driver in question.</summary>
    public string IdentityEvidenceNote { get; set; } = string.Empty;
    public bool IdentityVerified { get; set; }
    public Guid? IdentityVerifiedBy { get; set; }
    public DateTime? IdentityVerifiedAt { get; set; }

    public AppealStatus Status { get; set; } = AppealStatus.Received;

    /// <summary>
    /// The moderator handling the appeal. Must not be the moderator whose decision is under
    /// appeal — enforced in the resolve handler (clause 35.3).
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>Set while the appeal is open: the driver's public status is suppressed (clause 35.4).</summary>
    public bool PublicStatusSuspended { get; set; }

    public string? Outcome { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Clause 35.2 — the date by which we undertake to respond.</summary>
    public DateTime DueAt { get; set; } = DateTime.UtcNow.AddDays(30);

    public Driver Driver { get; set; } = null!;
}
