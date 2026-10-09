namespace RydrSafe.Domain.Enums;

/// <summary>Lifecycle of a driver appeal under user agreement Part C, clause 35.</summary>
public enum AppealStatus
{
    Received,
    UnderReview,
    Upheld,
    Dismissed,
    Withdrawn
}
