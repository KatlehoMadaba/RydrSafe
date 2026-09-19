using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Entities;

/// <summary>
/// Product feedback from a passenger — ideas, safety suggestions, usability notes.
/// Deliberately separate from <see cref="Report"/>: a report is an allegation about an
/// identifiable driver and carries moderation and legal weight, whereas this is a message to
/// the team about the product. Keeping them apart stops feedback from landing in the
/// moderation queue and stops product notes from touching anyone's risk score.
/// </summary>
public class Recommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public RecommendationCategory Category { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public RecommendationStatus Status { get; set; } = RecommendationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
