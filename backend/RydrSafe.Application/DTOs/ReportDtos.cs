namespace RydrSafe.Application.DTOs;

public record CreateReportRequest(
    string DriverName,
    string RegistrationNumber,
    string Category,
    string Severity,
    string Description,
    DateTime IncidentDate,
    bool ReportedToPolice = false,

    /// <summary>
    /// "Day", "Month" or "Year" — how precisely the reporter could place the incident. Anything
    /// coarser than Day means <c>IncidentDate</c> is the first instant of the period they named,
    /// not a date they claimed.
    /// </summary>
    string? IncidentDatePrecision = null,

    /// <summary>Optional SAPS CAS/AR number. Clause 6.3(b) — a moderator must still verify it.</summary>
    string? OfficialReference = null,

    /// <summary>
    /// Opaque client-generated device fingerprint. Hashed on arrival and used only for the
    /// clause 6.3(a) independence check. Disclosed in Part B clause 20.1.
    /// </summary>
    string? DeviceFingerprint = null,

    /// <summary>
    /// Clause 6.4. Withhold the reporter's name from the moderation queue. The report stays
    /// linked to the account underneath, because corroboration and abuse handling depend on it —
    /// the report form says so rather than promising an anonymity we do not provide.
    /// </summary>
    bool IsAnonymous = false
);

/// <summary>
/// The full report. Only ever returned to the reporter who submitted it, or to a moderator.
/// Carries the free-text description, so it must never be reachable from a public route.
/// </summary>
public record ReportDto(
    Guid Id,
    Guid DriverId,
    string DriverName,

    /// <summary>Null once the reporter's account has been deleted (clause 29.2).</summary>
    Guid? UserId,

    /// <summary>
    /// The reporter's name, or "Deleted account" once the report has been de-identified.
    /// Moderators still see it on an anonymous report — they need it for clause 37 abuse
    /// handling — and <see cref="IsAnonymous"/> tells the queue to mark the request. Never
    /// reachable from a route any other user can call.
    /// </summary>
    string ReporterName,

    /// <summary>Clause 6.4 — the reporter asked not to be named publicly.</summary>
    bool IsAnonymous,
    string Category,
    string Classification,
    string Severity,
    string Description,
    DateTime IncidentDate,

    /// <summary>"Day", "Month" or "Year". Tells the UI how much of the date to render.</summary>
    string IncidentDatePrecision,

    bool ReportedToPolice,
    string Status,
    string CorroborationPath,
    string? OfficialReference,
    bool OfficialReferenceVerified,
    DateTime? CorroboratedAt,
    DateTime CreatedAt
);

/// <summary>
/// Clause 6.4 — everything another user is permitted to see about reports against a driver:
/// a category, a severity band, a count, and a date range. No description, no reporter, no
/// per-report identity.
/// </summary>
public record PublicReportSummaryDto(
    string Category,
    string SeverityBand,
    int CorroboratedCount,
    DateTime? EarliestIncident,
    DateTime? LatestIncident
);

/// <summary>Moderator decision payload. Clause 7.3(c) requires a reason and an actor on every change.</summary>
public record ModerateReportRequest(
    string Reason,
    bool ReviewedReportContent = false,
    bool ReviewedDriverResponse = false,
    bool ReviewedRiskScore = false
);

/// <summary>Clause 6.3(b). A moderator confirms an official reference is well-formed and consistent.</summary>
public record VerifyOfficialReferenceRequest(string Reason, bool Verified);

/// <summary>Clause 6.3(c). A moderator records and validates a public-record source.</summary>
public record VerifyPublicRecordRequest(
    string SourceType,
    string? SourceUrl,
    string? SourceReference,
    string Reason
);

/// <summary>Clause 6.1. Reclassifying an <c>Other</c> report between Category A and B.</summary>
public record ReclassifyReportRequest(string Classification, string Reason);

public record ReportStatusAuditDto(
    Guid Id,
    Guid ActorUserId,
    string FromStatus,
    string ToStatus,
    string Reason,
    bool ReviewedReportContent,
    bool ReviewedDriverResponse,
    bool ReviewedRiskScore,
    DateTime CreatedAt
);

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
