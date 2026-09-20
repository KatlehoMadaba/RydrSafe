using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.Reports.Commands;

/// <summary>
/// Clause 6.3(b). A moderator confirms a SAPS CAS/AR number is well-formed and consistent with
/// the report. This is a corroboration input, so it is audited like any other decision.
/// </summary>
public record VerifyOfficialReferenceCommand(
    Guid ReportId,
    bool Verified,
    Guid ActorUserId,
    string Reason) : IRequest;

public class VerifyOfficialReferenceCommandValidator : AbstractValidator<VerifyOfficialReferenceCommand>
{
    public VerifyOfficialReferenceCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class VerifyOfficialReferenceCommandHandler(
    IReportRepository reportRepository,
    IAuditRepository auditRepository,
    ICorroborationService corroborationService) : IRequestHandler<VerifyOfficialReferenceCommand>
{
    public async Task Handle(VerifyOfficialReferenceCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        if (string.IsNullOrWhiteSpace(report.OfficialReference))
            throw new InvalidOperationException("This report carries no official reference to verify.");

        report.OfficialReferenceVerified = request.Verified;
        report.OfficialReferenceVerifiedBy = request.ActorUserId;
        report.OfficialReferenceVerifiedAt = DateTime.UtcNow;
        await reportRepository.UpdateAsync(report);

        var outcome = request.Verified ? "verified" : "rejected";

        await auditRepository.AddReportAuditAsync(new ReportStatusAudit
        {
            ReportId = report.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = report.Status,
            ToStatus = report.Status,
            Reason = $"Official reference {outcome}. {request.Reason}",
            ReviewedReportContent = true
        });

        await corroborationService.ReevaluateDriverAsync(
            report.DriverId, request.ActorUserId,
            $"Official reference on report {report.Id} {outcome}.");
    }
}

/// <summary>
/// Clause 6.3(c). A moderator records the public-record source relied on, so the basis for
/// publication is auditable rather than a moderator's recollection.
/// </summary>
public record RecordPublicRecordSourceCommand(
    Guid ReportId,
    string SourceType,
    string? SourceUrl,
    string? SourceReference,
    Guid ActorUserId,
    string Reason) : IRequest;

public class RecordPublicRecordSourceCommandValidator : AbstractValidator<RecordPublicRecordSourceCommand>
{
    /// <summary>
    /// Clause 6.3(c) admits only records a court, tribunal or regulator lawfully published, or
    /// which the data subject deliberately made public. Anything else is not a public record.
    /// </summary>
    public static readonly string[] AllowedSourceTypes =
        ["Court", "Tribunal", "Regulator", "SelfPublishedByDataSubject"];

    public RecordPublicRecordSourceCommandValidator()
    {
        RuleFor(x => x.SourceType).NotEmpty()
            .Must(AllowedSourceTypes.Contains)
            .WithMessage("Source type must be one of: " + string.Join(", ", AllowedSourceTypes) + ".");

        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.ActorUserId).NotEmpty();

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.SourceUrl) || !string.IsNullOrWhiteSpace(x.SourceReference))
            .WithMessage("A public-record source needs either a URL or a citable reference.");
    }
}

public class RecordPublicRecordSourceCommandHandler(
    IReportRepository reportRepository,
    IAuditRepository auditRepository,
    ICorroborationService corroborationService) : IRequestHandler<RecordPublicRecordSourceCommand>
{
    public async Task Handle(RecordPublicRecordSourceCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        report.PublicRecordSourceType = request.SourceType;
        report.PublicRecordSourceUrl = request.SourceUrl;
        report.PublicRecordSourceReference = request.SourceReference;
        report.PublicRecordVerifiedBy = request.ActorUserId;
        report.PublicRecordVerifiedAt = DateTime.UtcNow;
        await reportRepository.UpdateAsync(report);

        await auditRepository.AddReportAuditAsync(new ReportStatusAudit
        {
            ReportId = report.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = report.Status,
            ToStatus = report.Status,
            Reason = $"Public-record source recorded ({request.SourceType}). {request.Reason}",
            ReviewedReportContent = true
        });

        await corroborationService.ReevaluateDriverAsync(
            report.DriverId, request.ActorUserId,
            $"Public-record source recorded for report {report.Id}.");
    }
}

/// <summary>
/// Clause 6.3 revocation. Pulls a previously accepted corroboration source, which drops any
/// report relying on it back out of public view.
/// </summary>
public record RevokeCorroborationSourceCommand(
    Guid ReportId,
    Guid ActorUserId,
    string Reason) : IRequest;

public class RevokeCorroborationSourceCommandValidator : AbstractValidator<RevokeCorroborationSourceCommand>
{
    public RevokeCorroborationSourceCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class RevokeCorroborationSourceCommandHandler(
    IReportRepository reportRepository,
    IAuditRepository auditRepository,
    ICorroborationService corroborationService) : IRequestHandler<RevokeCorroborationSourceCommand>
{
    public async Task Handle(RevokeCorroborationSourceCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Report not found.");

        report.OfficialReferenceVerified = false;
        report.OfficialReferenceVerifiedBy = null;
        report.OfficialReferenceVerifiedAt = null;
        report.PublicRecordSourceType = null;
        report.PublicRecordSourceUrl = null;
        report.PublicRecordSourceReference = null;
        report.PublicRecordVerifiedBy = null;
        report.PublicRecordVerifiedAt = null;
        await reportRepository.UpdateAsync(report);

        await auditRepository.AddReportAuditAsync(new ReportStatusAudit
        {
            ReportId = report.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = report.Status,
            ToStatus = report.Status,
            Reason = $"Corroboration source revoked. {request.Reason}",
            ReviewedReportContent = true
        });

        await corroborationService.ReevaluateDriverAsync(
            report.DriverId, request.ActorUserId,
            $"Corroboration source revoked on report {report.Id}.");
    }
}
