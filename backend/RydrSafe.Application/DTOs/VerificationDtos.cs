namespace RydrSafe.Application.DTOs;

public record VerificationResponse(
    string? DriverName,
    string? RegistrationNumber,
    string? PhoneNumber,
    string Status,
    int RiskScore,
    int ReportCount,
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
