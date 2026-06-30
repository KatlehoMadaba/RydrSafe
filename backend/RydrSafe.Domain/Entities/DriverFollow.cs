namespace RydrSafe.Domain.Entities;

public class DriverFollow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
}
