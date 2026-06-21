using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

public class Driver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DriverName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int RiskScore { get; set; } = 0;
    public DriverStatus Status { get; set; } = DriverStatus.Safe;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Vehicle> Vehicles { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
}
