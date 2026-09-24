namespace RydrSafe.Application.DTOs;

/// <summary>Part D. One entry per checkbox the user ticked at sign-up.</summary>
public record ConsentAcceptance(string ConsentKey, bool Accepted);

/// <summary>
/// Part C clause 34. What a driver gets back from the self-check route. Deliberately
/// narrow: it confirms what we hold about them and nothing about who reported it.
/// </summary>
public record DriverSelfCheckResponse(
    bool RecordExists,
    string? DriverName,
    string? PublicStatus,
    int CorroboratedReportCount,
    IEnumerable<PublicReportSummaryDto> Summaries,
    DateTime? FirstSeen,
    string Message
);

public record DriverSelfCheckRequest(
    string RegistrationNumber,
    string? DriverName,
    string ContactEmail
);

public record CreateAppealRequest(
    string RegistrationNumber,
    string Grounds,
    string Detail,
    string ContactEmail,
    string? ContactPhone,
    string IdentityEvidenceNote
);

public record AppealDto(
    Guid Id,
    Guid DriverId,
    string DriverName,
    string Grounds,
    string Detail,
    string ContactEmail,
    string Status,
    bool IdentityVerified,
    bool PublicStatusSuspended,
    string? Outcome,
    DateTime CreatedAt,
    DateTime DueAt,
    DateTime? ResolvedAt
);

public record ResolveAppealRequest(
    string Status,
    string Outcome,
    bool ReviewedRiskScore = false,
    bool ReviewedDriverResponse = false,
    bool ReviewedScoringLogic = false
);

/// <summary>Clause 6.5. The driver's answer to a proposed status change.</summary>
public record RightOfReplyRequest(string RegistrationNumber, string Response, string ContactEmail);
