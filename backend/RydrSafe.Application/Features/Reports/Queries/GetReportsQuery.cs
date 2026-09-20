using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Reports.Queries;

/// <summary>Moderation queue. Moderators and administrators only — carries descriptions.</summary>
public record GetReportsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReportDto>>;

public class GetReportsQueryHandler(
    IReportRepository reportRepository) : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
{
    public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await reportRepository.GetAllAsync(request.Page, request.PageSize);

        // Previously this reported the page size as the total, so the moderator UI could never
        // page past the first screen.
        var total = await reportRepository.CountAllAsync();

        var dtos = reports.Select(GetReportByIdQueryHandler.Map).ToList();

        return new PagedResult<ReportDto>(dtos, total, request.Page, request.PageSize);
    }
}

/// <summary>A reporter's own submissions. Safe to return in full — they wrote them.</summary>
public record GetMyReportsQuery(Guid UserId) : IRequest<IEnumerable<ReportDto>>;

public class GetMyReportsQueryHandler(
    IReportRepository reportRepository) : IRequestHandler<GetMyReportsQuery, IEnumerable<ReportDto>>
{
    public async Task<IEnumerable<ReportDto>> Handle(GetMyReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await reportRepository.GetByUserIdAsync(request.UserId);
        return reports.Select(GetReportByIdQueryHandler.Map).ToList();
    }
}
