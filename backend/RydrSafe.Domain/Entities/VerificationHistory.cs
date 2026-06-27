namespace RydrSafe.Domain.Entities;

public class VerificationHistory 
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Status { get; set; } = "Safe";
    public int RiskScore { get; set; }
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
