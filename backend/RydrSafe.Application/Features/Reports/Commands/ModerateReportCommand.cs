using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Commands;

/// <summary>
/// Clause 7.3(c) and POPIA s71(2). A report's status only moves on a named human decision that
/// carries a reason and evidence the moderator actually looked at the material. There is no
/// code path that changes a report status without writing an audit row.
/// </summary>
public record ModerateReportCommand(
    Guid ReportId,
    ReportStatus TargetStatus,
    Guid ActorUserId,
    string Reason,
    bool ReviewedReportContent,
    bool ReviewedDriverResponse,
    bool ReviewedRiskScore,
    Guid? RelatedAppealId = null) : IRequest;

public class ModerateReportCommandValidator : AbstractValidator<ModerateReportCommand>
{
    public ModerateReportCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(2000)
            .WithMessage("A decision reason of at least 10 characters is required (clause 7.3(c)).");

        RuleFor(x => x.ActorUserId).NotEmpty();

        // POPIA s71(2)(b) asks for meaningful human involvement. A tick-free approval is a
        // rubber stamp, so we require the moderator to affirm they read the report itself.
        RuleFor(x => x.ReviewedReportContent).Equal(true)
            .WithMessage("You must confirm you reviewed the report content before deciding.");

        RuleFor(x => x.TargetStatus)
            .Must(s => s is ReportStatus.Approved or ReportStatus.Rejected)
            .WithMessage("Moderators may only approve or reject. Corroboration is decided by clause 6.3, not by hand.");
    }
}

public class ModerateReportCommandHandler(
    IReportRepository reportRepository,
    IAuditRepository auditRepository,
    ICorroborationService corroborationService,
    ICategoryAGate categoryAGate) : IRequestHandler<ModerateReportCommand>
{
    public async Task Handle(ModerateReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        if (report.Classification == ReportClassification.CategoryA && !categoryAGate.IsProcessingEnabled)
            throw new CategoryAProcessingDisabledException(categoryAGate.DisabledReason);

        if (report.Status is ReportStatus.Rejected or ReportStatus.Withdrawn)
            throw new InvalidStateTransitionException(
                $"Report is {report.Status} and cannot be moderated further.");

        var from = report.Status;
        report.Status = request.TargetStatus;

        if (request.TargetStatus == ReportStatus.Rejected)
        {
            // A rejected report can no longer support anyone else's corroboration.
            report.CorroborationPath = CorroborationPath.None;
            report.CorroboratedAt = null;
            report.CorroboratedBy = null;
        }

        await reportRepository.UpdateAsync(report);

        await auditRepository.AddReportAuditAsync(new ReportStatusAudit
        {
            ReportId = report.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = from,
            ToStatus = request.TargetStatus,
            Reason = request.Reason,
            ReviewedReportContent = request.ReviewedReportContent,
            ReviewedDriverResponse = request.ReviewedDriverResponse,
            ReviewedRiskScore = request.ReviewedRiskScore,
            RelatedAppealId = request.RelatedAppealId
        });

        // Approving this report may corroborate another one, and rejecting it may pull the
        // support out from under one that was already corroborated. Both directions are handled.
        await corroborationService.ReevaluateDriverAsync(
            report.DriverId,
            request.ActorUserId,
            $"Triggered by moderation of report {report.Id} ({from} -> {request.TargetStatus}).");
    }
}
