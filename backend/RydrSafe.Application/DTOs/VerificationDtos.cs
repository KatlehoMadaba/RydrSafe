namespace RydrSafe.Application.DTOs;

public record ManualVerificationRequest(
    string? RegistrationNumber,
    string? DriverName,
    string? PhoneNumber
);

public record VerificationResponse(
    string? DriverName,
    string? RegistrationNumber,
    string? PhoneNumber,
    string Status,
    int RiskScore,
    int ReportCount,

    /// <summary>
    /// Reports filed but not yet reviewed. Disclosed so a driver with unread reports is not
    /// presented as clear, and excluded from <see cref="RiskScore"/> and the status band on
    /// purpose — no finding has been made about them (clauses 6.2, 7.1).
    /// </summary>
    int PendingReportCount,

    /// <summary>
    /// Highest severity among those unreviewed reports, as chosen by the reporters. Null when
    /// there are none. Reporter-selected, never assessed — the UI must not present it as a
    /// RydrSafe finding.
    /// </summary>
    string? PendingHighestSeverity,

    bool MatchFound,
    Guid? DriverId
);

public record VerificationHistoryDto(
    Guid Id,
    string? DriverName,
    string? RegistrationNumber,
    string Status,
    int RiskScore,
    DateTime VerifiedAt
);

public record VerificationStatsDto(int Total, int Flagged, int Safe);
