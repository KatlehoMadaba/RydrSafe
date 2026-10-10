using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Queries;

/// <summary>
/// The one place that decides which of a driver's reports may be shown to another user.
///
/// Three views now read the same set — the grouped clause 6.4 summary, the per-incident list,
/// and the community-reports browser — and they must not be able to drift apart. A view that
/// quietly included a Category A report while the s58(2) standstill is in force would publish
/// what the gate exists to withhold.
/// </summary>
internal static class PublishableReports
{
    /// <summary>
    /// Corroborated reports only (clause 6.2), and while Category A publication is switched off,
    /// Category A reports are excluded outright rather than shown as a zero count.
    /// </summary>
    internal static IEnumerable<Report> Filter(
        IEnumerable<Report> reports, bool categoryAPublicationEnabled) =>
        reports.Where(r =>
            r.Status == ReportStatus.Corroborated
            && (categoryAPublicationEnabled || r.Classification == ReportClassification.CategoryB));
}
