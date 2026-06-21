namespace RydrSafe.Application.DTOs;

public record DriverDto(
    Guid Id,
    string DriverName,
    string? PhoneNumber,
    int RiskScore,
    string Status,
    int ReportCount,
    IEnumerable<VehicleDto> Vehicles,
    DateTime CreatedAt
);

public record VehicleDto(
    Guid Id,
    string RegistrationNumber,
    string? Make,
    string? Model,
    string? Color
);

public record DriverListDto(
    Guid Id,
    string DriverName,
    int RiskScore,
    string Status,
    int ReportCount,
    string? RegistrationNumber
);
