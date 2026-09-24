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

    /// <summary>
    /// Clause 35.4 — set while an appeal is open, or where a status decision is otherwise
    /// contested. While true the driver's public status is reported as <c>Safe</c> regardless
    /// of <see cref="Status"/>.
    /// </summary>
    public bool PublicStatusSuspended { get; set; }

    /// <summary>Clause 6.5. When the driver was last offered a right of reply before a status change.</summary>
    public DateTime? RightOfReplyOfferedAt { get; set; }

    /// <summary>The driver's response to a pending status change, considered by a moderator before it takes effect.</summary>
    public string? RightOfReplyResponse { get; set; }
    public DateTime? RightOfReplyRespondedAt { get; set; }

    /// <summary>Clause 21 (POPIA s18) — when we gave this driver notice that we hold information about them.</summary>
    public DateTime? Section18NoticeSentAt { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<DriverAppeal> Appeals { get; set; } = [];

    /// <summary>What other users are allowed to see. Suspended drivers read as <c>Safe</c> (clause 35.4).</summary>
    public DriverStatus PublicStatus => PublicStatusSuspended ? DriverStatus.Safe : Status;
}
