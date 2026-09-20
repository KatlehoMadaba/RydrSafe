namespace RydrSafe.Domain.Enums;

/// <summary>
/// Publication lifecycle for a report, per user agreement clause 6.2.
/// Only <see cref="Corroborated"/> is ever visible to users other than the reporter
/// and the moderation team.
/// </summary>
public enum ReportStatus
{
    /// <summary>Newly submitted, awaiting moderation. Visible to the reporter and moderators only.</summary>
    Pending,

    /// <summary>
    /// A moderator is satisfied the report is genuine. Still private: for Category A this has
    /// no public effect at all until the clause 6.3 corroboration threshold is met.
    /// </summary>
    Approved,

    /// <summary>
    /// The clause 6.3 corroboration threshold is met. This is the only state that surfaces
    /// publicly (in the reduced form of clause 6.4) and the only state that feeds the risk score.
    /// </summary>
    Corroborated,

    /// <summary>Rejected by a moderator. Terminal. Excluded from all counts and scoring.</summary>
    Rejected,

    /// <summary>Withdrawn by the reporter. Terminal. Excluded from all counts and scoring.</summary>
    Withdrawn
}
