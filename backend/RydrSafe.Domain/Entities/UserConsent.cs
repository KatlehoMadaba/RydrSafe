namespace RydrSafe.Domain.Entities;

/// <summary>
/// Part D. One row per checkbox per acceptance, so we can show exactly what a user agreed to,
/// which version of the agreement they saw, and when. Append-only: a withdrawal writes
/// <see cref="WithdrawnAt"/> on the existing row rather than deleting it.
/// Kept separate from driver-report acceptances.
/// </summary>
public class UserConsent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    /// <summary>Stable key for the checkbox, e.g. <c>terms.parts-a-to-c</c>. See <c>ConsentKeys</c>.</summary>
    public string ConsentKey { get; set; } = string.Empty;

    /// <summary>Version of the user agreement presented at the time, e.g. "1.0".</summary>
    public string AgreementVersion { get; set; } = string.Empty;

    /// <summary>Locale the agreement was presented in, e.g. "en-ZA".</summary>
    public string Locale { get; set; } = "en-ZA";

    /// <summary>Where it was collected, e.g. "web/register". Lets us distinguish sign-up from re-acceptance.</summary>
    public string CollectionSurface { get; set; } = string.Empty;

    public bool Accepted { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address at the moment of acceptance, retained as evidence that the agreement was
    /// concluded electronically (ECTA s11). Purged by the retention worker per clause 29.
    /// </summary>
    public string? IpAddress { get; set; }

    public DateTime? WithdrawnAt { get; set; }

    public User User { get; set; } = null!;
}

/// <summary>Stable keys for the Part D checkboxes. Changing a value breaks the consent audit trail.</summary>
public static class ConsentKeys
{
    /// <summary>Required. Acceptance of Parts A, B and C.</summary>
    public const string PartsAToC = "terms.parts-a-to-c";

    /// <summary>Required. Separate, conspicuous acknowledgement of the clause 15 risk limitation (CPA s49).</summary>
    public const string Clause15RiskLimitation = "terms.clause-15-risk-limitation";

    /// <summary>Required. Confirmation the account holder is 18 or older (clause 3.1).</summary>
    public const string AgeEighteenPlus = "eligibility.age-18-plus";

    /// <summary>Required. Acknowledgement that a knowingly false report may be actionable (clause 8).</summary>
    public const string FalseReportingConsequences = "reporting.false-report-consequences";

    /// <summary>Required. Acknowledgement of how screenshots are handled (clause 25).</summary>
    public const string ScreenshotHandling = "privacy.screenshot-handling";

    /// <summary>Required. Acknowledgement that a risk score is not proof of criminal conduct (clause 7).</summary>
    public const string RiskScoreNotProof = "verification.risk-score-not-proof";

    public static readonly string[] Required =
    [
        PartsAToC,
        Clause15RiskLimitation,
        AgeEighteenPlus,
        FalseReportingConsequences,
        ScreenshotHandling,
        RiskScoreNotProof
    ];
}
