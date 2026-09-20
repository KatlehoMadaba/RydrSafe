using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Services;

/// <summary>
/// The outcome of testing a report against clause 6.3.
/// </summary>
/// <param name="Path">Which limb was satisfied, if any.</param>
/// <param name="Reason">Human-readable explanation, recorded on the audit row.</param>
public record CorroborationResult(CorroborationPath Path, string Reason)
{
    public bool IsCorroborated => Path != CorroborationPath.None;

    public static CorroborationResult NotCorroborated(string reason) =>
        new(CorroborationPath.None, reason);
}

/// <summary>
/// User agreement clause 6.3 — the single safeguard the platform's legal position rests on.
/// A Category A report may not be shown to other users on the strength of one account's say-so.
/// </summary>
public static class CorroborationPolicy
{
    /// <summary>
    /// Two reports are treated as independent unless they share a reporter, or share a device or
    /// network signal. These are the only linkage signals RydrSafe collects, and they are
    /// disclosed in Part B clause 20.1.
    ///
    /// Reporter identity is tested through the pseudonymous key first, because that is what
    /// survives account deletion. Without it, two reports from one closed account would both
    /// carry a null <c>UserId</c> and could corroborate each other — the exact collusion
    /// clause 6.3(a) exists to catch.
    /// </summary>
    public static bool AreIndependent(Report a, Report b)
    {
        if (!string.IsNullOrEmpty(a.ReporterKeyHash) && a.ReporterKeyHash == b.ReporterKeyHash)
            return false;

        if (a.UserId is not null && a.UserId == b.UserId) return false;

        // Both reporters are unknown and at least one predates the pseudonymous key, so there is
        // nothing left to tell these two apart. Independence has to be shown, not assumed: the
        // cost of guessing wrong here is publishing an uncorroborated criminal allegation.
        if (a.UserId is null && b.UserId is null
            && (string.IsNullOrEmpty(a.ReporterKeyHash) || string.IsNullOrEmpty(b.ReporterKeyHash)))
            return false;

        if (!string.IsNullOrEmpty(a.SubmissionDeviceHash)
            && a.SubmissionDeviceHash == b.SubmissionDeviceHash) return false;

        if (!string.IsNullOrEmpty(a.SubmissionIpHash)
            && a.SubmissionIpHash == b.SubmissionIpHash) return false;

        return true;
    }

    /// <summary>
    /// Tests <paramref name="report"/> against all three limbs of clause 6.3.
    /// <paramref name="siblings"/> must be the other live reports naming the same driver.
    /// </summary>
    public static CorroborationResult Evaluate(Report report, IReadOnlyCollection<Report> siblings)
    {
        if (report.Classification == ReportClassification.CategoryB)
            return new CorroborationResult(
                CorroborationPath.IndependentReports,
                "Category B: a single approved report may be shown in generalised form (clause 6.3).");

        // 6.3(b) — official reference, confirmed by a moderator.
        if (report.OfficialReferenceVerified && !string.IsNullOrWhiteSpace(report.OfficialReference))
            return new CorroborationResult(
                CorroborationPath.OfficialReference,
                $"Official reference {report.OfficialReference} verified by a moderator (clause 6.3(b)).");

        // 6.3(c) — matter of public record, from a validated source.
        if (report.PublicRecordVerifiedBy is not null
            && !string.IsNullOrWhiteSpace(report.PublicRecordSourceType))
            return new CorroborationResult(
                CorroborationPath.PublicRecord,
                $"Public record source ({report.PublicRecordSourceType}) validated by a moderator (clause 6.3(c)).");

        // 6.3(a) — a second independent approved report in the same or a related category.
        var independent = siblings.FirstOrDefault(s =>
            s.Id != report.Id
            && s.Classification == ReportClassification.CategoryA
            && s.Status is ReportStatus.Approved or ReportStatus.Corroborated
            && IsSameOrRelated(s.Category, report.Category)
            && AreIndependent(report, s));

        if (independent is not null)
            return new CorroborationResult(
                CorroborationPath.IndependentReports,
                $"Independent approved report {independent.Id} names the same driver in a related category (clause 6.3(a)).");

        return CorroborationResult.NotCorroborated(
            "No corroborating report, official reference or public record. Held privately under clause 6.3.");
    }

    /// <summary>
    /// Clause 6.3(a) accepts "the same or a related category". Related means the same harm type:
    /// dishonesty offences corroborate each other, as do the two driving-conduct categories.
    /// </summary>
    public static bool IsSameOrRelated(ReportCategory a, ReportCategory b)
    {
        if (a == b) return true;

        return (Family(a), Family(b)) switch
        {
            (var x, var y) when x == y && x != CategoryFamily.Unrelated => true,
            _ => false
        };
    }

    private enum CategoryFamily { Dishonesty, DrivingConduct, PersonalSafety, Unrelated }

    private static CategoryFamily Family(ReportCategory category) => category switch
    {
        ReportCategory.Theft or ReportCategory.Fraud => CategoryFamily.Dishonesty,
        ReportCategory.RecklessDriving or ReportCategory.IntoxicatedDriving => CategoryFamily.DrivingConduct,
        ReportCategory.Assault or ReportCategory.Harassment => CategoryFamily.PersonalSafety,
        _ => CategoryFamily.Unrelated
    };
}
