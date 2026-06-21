using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Commands;

public record UpdateReportStatusCommand(Guid ReportId, ReportStatus Status) : IRequest;

public class UpdateReportStatusCommandHandler(
    IReportRepository reportRepository) : IRequestHandler<UpdateReportStatusCommand>
{
    public async Task Handle(UpdateReportStatusCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        report.Status = request.Status;
        await reportRepository.UpdateAsync(report);
    }
}
