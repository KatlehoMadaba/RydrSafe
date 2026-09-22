using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Queries;

/// <summary>
/// Clause 6.4 — the only shape in which report data may be shown to someone other than the
/// reporter or a moderator: category, severity band, corroborated count, incident date range.
/// No description, no reporter, no per-report identifiers.
/// </summary>
public record GetPublicReportSummariesQuery(Guid DriverId) : IRequest<IEnumerable<PublicReportSummaryDto>>;

public class GetPublicReportSummariesQueryHandler(
    IReportRepository reportRepository,
    ICategoryAGate categoryAGate)
    : IRequestHandler<GetPublicReportSummariesQuery, IEnumerable<PublicReportSummaryDto>>
{
    public async Task<IEnumerable<PublicReportSummaryDto>> Handle(
        GetPublicReportSummariesQuery request, CancellationToken cancellationToken)
    {
        var corroborated = await reportRepository.GetCorroboratedByDriverIdAsync(request.DriverId);
        return Build(corroborated, categoryAGate.IsPublicationEnabled);
    }

    /// <summary>
    /// Groups corroborated reports into the clause 6.4 shape. While Category A publication is
    /// switched off, Category A reports are excluded entirely rather than shown as a zero count.
    /// </summary>
    internal static List<PublicReportSummaryDto> Build(
        IEnumerable<Report> corroborated, bool categoryAPublicationEnabled)
    {
        var visible = corroborated.Where(r =>
            r.Status == ReportStatus.Corroborated
            && (categoryAPublicationEnabled || r.Classification == ReportClassification.CategoryB));

        return visible
            .GroupBy(r => r.Category)
            .Select(g => new PublicReportSummaryDto(
                g.Key.ToString(),
                // A band, not a per-report severity: the highest severity in the group.
                g.Max(r => r.Severity).ToString(),
                g.Count(),
                g.Min(r => r.IncidentDate),
                g.Max(r => r.IncidentDate)))
            .OrderByDescending(s => s.CorroboratedCount)
            .ToList();
    }
}

/// <summary>Clause 7.3(c) — the decision trail on a report. Moderators and administrators only.</summary>
public record GetReportAuditsQuery(Guid ReportId) : IRequest<IEnumerable<ReportStatusAuditDto>>;

public class GetReportAuditsQueryHandler(
    IAuditRepository auditRepository) : IRequestHandler<GetReportAuditsQuery, IEnumerable<ReportStatusAuditDto>>
{
    public async Task<IEnumerable<ReportStatusAuditDto>> Handle(
        GetReportAuditsQuery request, CancellationToken cancellationToken)
    {
        var audits = await auditRepository.GetReportAuditsAsync(request.ReportId);

        return audits.Select(a => new ReportStatusAuditDto(
            a.Id,
            a.ActorUserId,
            a.FromStatus.ToString(),
            a.ToStatus.ToString(),
            a.Reason,
            a.ReviewedReportContent,
            a.ReviewedDriverResponse,
            a.ReviewedRiskScore,
            a.CreatedAt)).ToList();
    }
}
