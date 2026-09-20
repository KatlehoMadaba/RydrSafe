namespace RydrSafe.Application.Common.Interfaces;

/// <summary>
/// Clause 29. The retention periods the platform actually enforces, read from configuration so
/// they can be shortened on legal advice without a code change. Exposed as an interface so the
/// application layer can set a row's expiry without taking a dependency on configuration.
/// </summary>
public interface IRetentionPolicy
{
    /// <summary>How long OCR-derived verification data is kept before the worker strips it.</summary>
    int VerificationOcrMonths { get; }

    /// <summary>How long an IP address captured alongside consent is kept.</summary>
    int ConsentIpMonths { get; }

    /// <summary>How long driver-record access logs are kept.</summary>
    int AccessLogDays { get; }

    /// <summary>The expiry to stamp on a verification row created now.</summary>
    DateTime VerificationOcrExpiry(DateTime from);
}
