namespace RydrSafe.Domain.Enums;

/// <summary>Which limb of user agreement clause 6.3 carried a report to <c>Corroborated</c>.</summary>
public enum CorroborationPath
{
    /// <summary>Not corroborated.</summary>
    None,

    /// <summary>6.3(a) — two or more independent approved reports naming the same driver.</summary>
    IndependentReports,

    /// <summary>6.3(b) — a SAPS case number or other official reference, confirmed by a moderator.</summary>
    OfficialReference,

    /// <summary>6.3(c) — already a matter of public record, from a validated source.</summary>
    PublicRecord
}
