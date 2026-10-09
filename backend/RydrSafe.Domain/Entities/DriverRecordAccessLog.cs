namespace RydrSafe.Domain.Entities;

/// <summary>
/// Every access to a driver record through the unauthenticated self-check route is logged,
/// so enumeration and scraping are detectable after the fact (COMPLIANCE-NOTES B7).
/// No raw IP is stored — only a salted hash, which is enough to spot a pattern.
/// </summary>
public class DriverRecordAccessLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Null where the lookup found nothing — we still log the attempt.</summary>
    public Guid? DriverId { get; set; }

    /// <summary>Salted SHA-256 of the requesting IP.</summary>
    public string RequesterIpHash { get; set; } = string.Empty;

    /// <summary>Which surface was used, e.g. "driver-self-check".</summary>
    public string Surface { get; set; } = string.Empty;

    /// <summary>Salted hash of the lookup term, so repeated probing is visible without storing the term.</summary>
    public string LookupTermHash { get; set; } = string.Empty;

    /// <summary>Whether a record was returned. Response bodies are identical either way.</summary>
    public bool Matched { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
