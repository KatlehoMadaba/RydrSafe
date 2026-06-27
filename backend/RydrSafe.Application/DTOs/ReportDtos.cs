namespace RydrSafe.Application.DTOs;

public record CreateReportRequest(
    string DriverName,
    string RegistrationNumber,
    string Category,
    string Severity,
    string Description,
    DateTime IncidentDate
);

public record ReportDto(
    Guid Id,
    Guid DriverId,
    string DriverName,
    Guid UserId,
    string ReporterName,
    string Category,
    string Severity,
    string Description,
    DateTime IncidentDate,
    string Status,
    DateTime CreatedAt
);

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
