using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DriverId { get; set; }
    public Guid UserId { get; set; }
    public ReportCategory Category { get; set; }
    public ReportSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Driver Driver { get; set; } = null!;
    public User User { get; set; } = null!;
}
