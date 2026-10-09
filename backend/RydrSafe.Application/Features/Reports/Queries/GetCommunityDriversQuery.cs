using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Queries;

/// <summary>
/// The driver list behind the community-reports browser.
///
/// It deliberately does not reuse <c>GetDriversQuery</c>. That list leads with RydrSafe's own
/// risk score and status band; this page is about what passengers reported, so each driver
/// carries a tally of confirmed reports per severity band instead, plus the count of reports
/// nobody has reviewed yet.
///
/// <paramref name="Search"/> empty browses the most recent drivers; otherwise it searches.
/// </summary>
public record GetCommunityDriversQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<CommunityDriverDto>>;

public class GetCommunityDriversQueryHandler(
    IDriverRepository driverRepository,
    IReportRepository reportRepository,
    ICategoryAGate categoryAGate)
    : IRequestHandler<GetCommunityDriversQuery, PagedResult<CommunityDriverDto>>
{
    public async Task<PagedResult<CommunityDriverDto>> Handle(
        GetCommunityDriversQuery request, CancellationToken cancellationToken)
    {
        var searching = !string.IsNullOrWhiteSpace(request.Search);

        var drivers = searching
            ? (await driverRepository.SearchAsync(request.Search!)).ToList()
            : (await driverRepository.GetAllAsync(request.Page, request.PageSize)).ToList();

        var total = searching ? drivers.Count : await driverRepository.CountAsync();

        var dtos = new List<CommunityDriverDto>();
        foreach (var d in drivers)
        {
            var corroborated = await reportRepository.GetCorroboratedByDriverIdAsync(d.Id);
            var publishable = PublishableReports
                .Filter(corroborated, categoryAGate.IsPublicationEnabled)
                .ToList();

            // Worst band first: a passenger scanning the list is looking for the high ones.
            var severities = publishable
                .GroupBy(r => r.Severity)
                .OrderByDescending(g => g.Key)
                .Select(g => new SeverityTallyDto(g.Key.ToString(), g.Count()))
                .ToList();

            var pendingCount = await reportRepository.CountPendingByDriverIdAsync(d.Id);
            var pendingSeverity = pendingCount > 0
                ? await reportRepository.GetHighestPendingSeverityByDriverIdAsync(d.Id)
                : (ReportSeverity?)null;

            dtos.Add(new CommunityDriverDto(
                d.Id,
                d.DriverName,
                d.Vehicles.FirstOrDefault()?.RegistrationNumber,
                severities,
                publishable.Count,
                pendingCount,
                pendingSeverity?.ToString()));
        }

        return new PagedResult<CommunityDriverDto>(dtos, total, request.Page, request.PageSize);
    }
}
