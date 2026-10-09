using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

/// <summary>
/// Clause 7.3(c) and POPIA s71(2): every change to a report's status is attributable to a
/// named human, with a reason and evidence that the review was meaningful rather than a rubber stamp.
/// Append-only — rows are never updated or deleted.
/// </summary>
public class ReportStatusAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }

    /// <summary>The moderator or administrator who made the decision. Never null — automated changes are not permitted.</summary>
    public Guid ActorUserId { get; set; }

    public ReportStatus FromStatus { get; set; }
    public ReportStatus ToStatus { get; set; }

    /// <summary>Free-text reason recorded by the moderator. Required.</summary>
    public string Reason { get; set; } = string.Empty;

    // ---- Evidence of meaningful human involvement (POPIA s71(2)(b)) ----

    /// <summary>The moderator confirms they read the report and its supporting material.</summary>
    public bool ReviewedReportContent { get; set; }

    /// <summary>The moderator confirms they considered the driver's response, where one exists.</summary>
    public bool ReviewedDriverResponse { get; set; }

    /// <summary>The moderator confirms they reviewed the computed risk score and how it was derived.</summary>
    public bool ReviewedRiskScore { get; set; }

    /// <summary>Set where this decision resolves a driver appeal, linking the two records.</summary>
    public Guid? RelatedAppealId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Report Report { get; set; } = null!;
}
