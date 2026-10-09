namespace RydrSafe.Application.Common.Interfaces;

/// <summary>
/// Salted one-way hashes for values we need to compare but must not retain in the clear:
/// IP addresses and device fingerprints used for the clause 6.3(a) independence check, image
/// bytes used for duplicate detection, and lookup terms recorded in the driver access log.
/// </summary>
public interface IHashingService
{
    /// <summary>Salted SHA-256 of a short string. Returns null for null/blank input.</summary>
    string? HashIdentifier(string? value);

    /// <summary>Unsalted SHA-256 of image bytes, so the same image hashes alike across users.</summary>
    string HashBytes(ReadOnlySpan<byte> bytes);
}
