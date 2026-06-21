using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Reports.Queries;

public record GetReportsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReportDto>>;

public class GetReportsQueryHandler(
    IReportRepository reportRepository) : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
{
    public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await reportRepository.GetAllAsync(request.Page, request.PageSize);

        var dtos = reports.Select(r => new ReportDto(
            r.Id, r.DriverId, r.Driver?.DriverName ?? string.Empty,
            r.UserId, r.User?.FullName ?? string.Empty,
            r.Category.ToString(), r.Severity.ToString(),
            r.Description, r.IncidentDate, r.Status.ToString(), r.CreatedAt));

        return new PagedResult<ReportDto>(dtos, dtos.Count(), request.Page, request.PageSize);
    }
}
