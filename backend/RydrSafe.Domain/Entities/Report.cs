using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DriverId { get; set; }

    /// <summary>
    /// The account that submitted this report. Nullable because clause 29.2 keeps the report
    /// when its reporter closes their account — the row is de-identified rather than deleted,
    /// since it concerns a driver who may still be disputing it.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Clause 6.4. The reporter asked not to be named. Other users never see reporter identity
    /// in any case; this withholds the name from the moderation queue's face as well, and is
    /// recorded so we can show the request was honoured. The account link is kept regardless —
    /// clause 6.3(a) independence and clause 37 abuse handling both depend on it.
    /// </summary>
    public bool IsAnonymous { get; set; }

    /// <summary>
    /// Salted SHA-256 of the reporter's account id, written at submission. It exists so the
    /// clause 6.3(a) independence test still works after <see cref="UserId"/> is cleared: two
    /// de-identified reports from the same closed account must not corroborate each other.
    /// </summary>
    public string? ReporterKeyHash { get; set; }

    /// <summary>Set when <see cref="UserId"/> was cleared because the account was deleted (clause 29.2).</summary>
    public DateTime? ReporterDeletedAt { get; set; }
    public ReportCategory Category { get; set; }
    public ReportSeverity Severity { get; set; }

    /// <summary>
    /// Clause 6.1. Derived from <see cref="Category"/> at submission by
    /// <c>ReportClassificationPolicy</c>; a moderator may reclassify an <c>Other</c> report.
    /// </summary>
    public ReportClassification Classification { get; set; } = ReportClassification.CategoryA;

    /// <summary>Set when a moderator reclassifies an <c>Other</c> report. Clause 6.1.</summary>
    public Guid? ReclassifiedBy { get; set; }
    public DateTime? ReclassifiedAt { get; set; }

    /// <summary>Never published. Clause 6.4 — descriptions are visible to the reporter and moderators only.</summary>
    public string Description { get; set; } = string.Empty;

    public DateTime IncidentDate { get; set; }

    /// <summary>
    /// How much of <see cref="IncidentDate"/> the reporter actually knew. Defaults to
    /// <c>Day</c> so every report written before this existed keeps its original meaning.
    /// </summary>
    public IncidentDatePrecision IncidentDatePrecision { get; set; } = IncidentDatePrecision.Day;
    public bool ReportedToPolice { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ---- Clause 6.3(b): official reference ----

    /// <summary>SAPS CAS/AR number or other official reference supplied by the reporter.</summary>
    public string? OfficialReference { get; set; }

    /// <summary>A moderator has confirmed the reference is well-formed and consistent with the report.</summary>
    public bool OfficialReferenceVerified { get; set; }
    public Guid? OfficialReferenceVerifiedBy { get; set; }
    public DateTime? OfficialReferenceVerifiedAt { get; set; }

    // ---- Clause 6.3(c): public record ----

    /// <summary>Where the public-record corroboration came from (court, tribunal, regulator, or self-published).</summary>
    public string? PublicRecordSourceType { get; set; }
    public string? PublicRecordSourceUrl { get; set; }
    public string? PublicRecordSourceReference { get; set; }
    public Guid? PublicRecordVerifiedBy { get; set; }
    public DateTime? PublicRecordVerifiedAt { get; set; }

    // ---- Corroboration outcome ----

    public CorroborationPath CorroborationPath { get; set; } = CorroborationPath.None;
    public Guid? CorroboratedBy { get; set; }
    public DateTime? CorroboratedAt { get; set; }

    /// <summary>
    /// Set when corroboration is revoked because the supporting path was withdrawn or disproved
    /// (clause 6.3, and COMPLIANCE-NOTES B2). The report drops back to <c>Approved</c>.
    /// </summary>
    public DateTime? CorroborationRevokedAt { get; set; }
    public string? CorroborationRevokedReason { get; set; }

    public DateTime? WithdrawnAt { get; set; }

    // ---- Independence signals (clause 6.3(a), disclosed in Part B clause 20.1) ----

    /// <summary>
    /// Salted SHA-256 of the submitting IP address. Used only to tell whether two reports naming
    /// the same driver came from linked accounts. The raw address is never stored.
    /// </summary>
    public string? SubmissionIpHash { get; set; }

    /// <summary>
    /// Salted SHA-256 of a client-supplied device fingerprint, for the same purpose.
    /// Never stored raw, never used for any other decision.
    /// </summary>
    public string? SubmissionDeviceHash { get; set; }

    public Driver Driver { get; set; } = null!;

    /// <summary>Null once the reporter's account has been deleted. See <see cref="UserId"/>.</summary>
    public User? User { get; set; }
    public ICollection<ReportStatusAudit> StatusAudits { get; set; } = [];

    /// <summary>True where this report still stands — i.e. not rejected and not withdrawn.</summary>
    public bool IsLive => Status is not (ReportStatus.Rejected or ReportStatus.Withdrawn);
}
