using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Reports.Queries;

public record GetReportByIdQuery(Guid Id) : IRequest<ReportDto?>;

public class GetReportByIdQueryHandler(
    IReportRepository reportRepository) : IRequestHandler<GetReportByIdQuery, ReportDto?>
{
    public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await reportRepository.GetByIdAsync(request.Id);
        if (r is null) return null;

        return new ReportDto(
            r.Id, r.DriverId, r.Driver?.DriverName ?? string.Empty,
            r.UserId, r.User?.FullName ?? string.Empty,
            r.Category.ToString(), r.Severity.ToString(),
            r.Description, r.IncidentDate, r.Status.ToString(), r.CreatedAt);
    }
}
