namespace RydrSafe.Application.Common.Interfaces;

/// <summary>
/// COMPLIANCE-NOTES B5. POPIA s57(1)(b) requires prior authorisation from the Information
/// Regulator before processing information about alleged criminal conduct on behalf of third
/// parties, and s58(2) prohibits that processing until the Regulator has responded.
///
/// The standstill covers the whole lifecycle — submission, storage, moderation, matching,
/// scoring and publication — not publication alone. This gate is therefore checked at
/// submission, before anything is written.
///
/// Controlled by <c>CategoryA:ProcessingEnabled</c> in configuration so it can be switched
/// without a deployment.
/// </summary>
public interface ICategoryAGate
{
    /// <summary>True when Category A processing has a confirmed lawful path and is permitted.</summary>
    bool IsProcessingEnabled { get; }

    /// <summary>
    /// True when corroborated Category A reports may surface publicly. Always false while
    /// <see cref="IsProcessingEnabled"/> is false.
    /// </summary>
    bool IsPublicationEnabled { get; }

    /// <summary>Message shown to a user whose Category A report cannot be accepted yet.</summary>
    string DisabledReason { get; }
}
