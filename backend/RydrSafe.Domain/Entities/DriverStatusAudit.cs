using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

/// <summary>
/// Clause 7.3. The risk score may be computed automatically, but the public status band may
/// only change on a recorded human decision. Append-only.
/// </summary>
public class DriverStatusAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DriverId { get; set; }
    public Guid ActorUserId { get; set; }

    public DriverStatus FromStatus { get; set; }
    public DriverStatus ToStatus { get; set; }

    /// <summary>The score the moderator was shown when they made the decision.</summary>
    public int RiskScoreAtDecision { get; set; }

    public string Reason { get; set; } = string.Empty;

    public bool ReviewedRiskScore { get; set; }
    public bool ReviewedDriverResponse { get; set; }
    public bool ReviewedScoringLogic { get; set; }

    /// <summary>Whether the driver was given the clause 6.5 right of reply before this change.</summary>
    public bool RightOfReplyOffered { get; set; }
    public DateTime? RightOfReplyOfferedAt { get; set; }

    public Guid? RelatedAppealId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Driver Driver { get; set; } = null!;
}
