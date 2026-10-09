using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Queries;

/// <summary>
/// Clause 6.4, ungrouped. <see cref="GetPublicReportSummariesQuery"/> collapses a driver's
/// corroborated reports into one row per category; this returns them one card per report, so a
/// passenger reading a driver's page sees that three separate people reported the same thing
/// rather than a single line saying "3".
///
/// It discloses nothing the grouped view does not: same categories, same severities, same date
/// range, just not summed. No description and no reporter — clause 6.4 forbids both, and the
/// description is the part that carries defamatory detail.
/// </summary>
public record GetPublicReportIncidentsQuery(Guid DriverId)
    : IRequest<IEnumerable<PublicReportIncidentDto>>;

public class GetPublicReportIncidentsQueryHandler(
    IReportRepository reportRepository,
    ICategoryAGate categoryAGate)
    : IRequestHandler<GetPublicReportIncidentsQuery, IEnumerable<PublicReportIncidentDto>>
{
    public async Task<IEnumerable<PublicReportIncidentDto>> Handle(
        GetPublicReportIncidentsQuery request, CancellationToken cancellationToken)
    {
        var corroborated = await reportRepository.GetCorroboratedByDriverIdAsync(request.DriverId);
        return Build(corroborated, categoryAGate.IsPublicationEnabled);
    }

    internal static List<PublicReportIncidentDto> Build(
        IEnumerable<Report> corroborated, bool categoryAPublicationEnabled)
    {
        return PublishableReports.Filter(corroborated, categoryAPublicationEnabled)
            .OrderByDescending(r => r.IncidentDate)
            .ThenByDescending(r => r.Severity)
            .Select(r => new PublicReportIncidentDto(
                r.Category.ToString(),
                r.Severity.ToString(),
                r.CorroborationPath.ToString(),
                Coarsen(r.IncidentDate, r.IncidentDatePrecision),
                r.IncidentDatePrecision == IncidentDatePrecision.Year ? "Year" : "Month"))
            .ToList();
    }

    /// <summary>
    /// Drops a report's date to the start of its period. A reporter who named an exact day gets
    /// rounded to the month here: the grouped view only ever exposed a month-level range, and a
    /// per-incident list must not become the finer disclosure that clause 6.4 rules out.
    /// </summary>
    private static DateTime Coarsen(DateTime incidentDate, IncidentDatePrecision precision) =>
        precision == IncidentDatePrecision.Year
            ? new DateTime(incidentDate.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(incidentDate.Year, incidentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
}
