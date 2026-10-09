using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;

namespace RydrSafe.Application.Features.Reports.Commands;

/// <summary>
/// Clause 6.1. An <c>Other</c> report starts life as Category A; a moderator may move it to
/// Category B once they have read it and are satisfied it alleges no offence.
/// </summary>
public record ReclassifyReportCommand(
    Guid ReportId,
    ReportClassification Classification,
    Guid ActorUserId,
    string Reason) : IRequest;

public class ReclassifyReportCommandValidator : AbstractValidator<ReclassifyReportCommand>
{
    public ReclassifyReportCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class ReclassifyReportCommandHandler(
    IReportRepository reportRepository,
    IAuditRepository auditRepository,
    ICorroborationService corroborationService) : IRequestHandler<ReclassifyReportCommand>
{
    public async Task Handle(ReclassifyReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        // Only Other is reclassifiable. Letting a moderator downgrade "Assault" to Category B
        // would route an allegation of an offence straight past the clause 6.3 threshold.
        if (!ReportClassificationPolicy.CanReclassify(report.Category))
            throw new InvalidStateTransitionException(
                $"Only reports categorised as 'Other' may be reclassified. This report is '{report.Category}'.");

        if (report.Classification == request.Classification) return;

        report.Classification = request.Classification;
        report.ReclassifiedBy = request.ActorUserId;
        report.ReclassifiedAt = DateTime.UtcNow;
        await reportRepository.UpdateAsync(report);

        await auditRepository.AddReportAuditAsync(new ReportStatusAudit
        {
            ReportId = report.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = report.Status,
            ToStatus = report.Status,
            Reason = $"Reclassified to {request.Classification}. {request.Reason}",
            ReviewedReportContent = true
        });

        await corroborationService.ReevaluateDriverAsync(
            report.DriverId, request.ActorUserId,
            $"Report {report.Id} reclassified to {request.Classification}.");
    }
}
