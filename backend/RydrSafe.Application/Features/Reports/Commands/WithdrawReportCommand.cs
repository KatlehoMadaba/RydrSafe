using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Commands;

/// <summary>
/// Clause 6.2. A reporter may withdraw their own report at any time. Withdrawal is terminal and
/// removes the report from all counts and scoring — including any corroboration it was
/// supporting, which is re-evaluated immediately.
/// </summary>
public record WithdrawReportCommand(Guid ReportId, Guid RequestingUserId, string Reason) : IRequest;

public class WithdrawReportCommandValidator : AbstractValidator<WithdrawReportCommand>
{
    public WithdrawReportCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}

public class WithdrawReportCommandHandler(
    IReportRepository reportRepository,
    IAuditRepository auditRepository,
    ICorroborationService corroborationService) : IRequestHandler<WithdrawReportCommand>
{
    public async Task Handle(WithdrawReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        if (report.UserId != request.RequestingUserId)
            throw new ForbiddenException("You may only withdraw a report you submitted.");

        if (report.Status is ReportStatus.Rejected or ReportStatus.Withdrawn)
            throw new InvalidStateTransitionException($"Report is already {report.Status}.");

        var from = report.Status;
        report.Status = ReportStatus.Withdrawn;
        report.WithdrawnAt = DateTime.UtcNow;
        report.CorroborationPath = CorroborationPath.None;
        report.CorroboratedAt = null;
        report.CorroboratedBy = null;
        await reportRepository.UpdateAsync(report);

        await auditRepository.AddReportAuditAsync(new ReportStatusAudit
        {
            ReportId = report.Id,
            ActorUserId = request.RequestingUserId,
            FromStatus = from,
            ToStatus = ReportStatus.Withdrawn,
            Reason = $"Withdrawn by the reporter. {request.Reason}",
            ReviewedReportContent = true
        });

        await corroborationService.ReevaluateDriverAsync(
            report.DriverId, request.RequestingUserId,
            $"Report {report.Id} withdrawn by its reporter.");
    }
}
