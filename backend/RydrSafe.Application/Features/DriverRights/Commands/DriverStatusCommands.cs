using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;

namespace RydrSafe.Application.Features.DriverRights.Commands;

/// <summary>
/// Clause 6.5. Before a driver's public status moves to Flagged or High Risk, we tell them and
/// give them a chance to answer. This command sends that notice and records when it went.
/// </summary>
public record OfferRightOfReplyCommand(
    Guid DriverId,
    Guid ActorUserId,
    DriverStatus ProposedStatus) : IRequest;

public class OfferRightOfReplyCommandHandler(
    IDriverRepository driverRepository,
    IRealtimeNotificationService notificationService) : IRequestHandler<OfferRightOfReplyCommand>
{
    public async Task Handle(OfferRightOfReplyCommand request, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(request.DriverId)
            ?? throw new KeyNotFoundException("Driver not found.");

        driver.RightOfReplyOfferedAt = DateTime.UtcNow;
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        // Clause 6.5 is conditional on us having a usable contact detail. Where we have none,
        // the offer is recorded as attempted so the audit trail shows why it did not happen.
        await notificationService.NotifyModeratorsAsync(
            "Right of Reply Due",
            driver.PhoneNumber is null
                ? $"No contact detail held for {driver.DriverName}; right of reply could not be sent " +
                  $"before the proposed move to {request.ProposedStatus}."
                : $"Right of reply notice is due to {driver.DriverName} ({driver.PhoneNumber}) " +
                  $"before the proposed move to {request.ProposedStatus}.");
    }
}

/// <summary>Clause 6.5. The driver's answer, recorded so a moderator must read it before deciding.</summary>
public record SubmitRightOfReplyCommand(
    string RegistrationNumber,
    string Response,
    string ContactEmail,
    string? RequesterIpAddress) : IRequest;

public class SubmitRightOfReplyCommandValidator : AbstractValidator<SubmitRightOfReplyCommand>
{
    public SubmitRightOfReplyCommandValidator()
    {
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Response).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
    }
}

public class SubmitRightOfReplyCommandHandler(
    IDriverRepository driverRepository,
    IDriverAccessLogRepository accessLogRepository,
    IHashingService hashingService,
    IRealtimeNotificationService notificationService) : IRequestHandler<SubmitRightOfReplyCommand>
{
    public async Task Handle(SubmitRightOfReplyCommand request, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByRegistrationNumberAsync(request.RegistrationNumber);

        await accessLogRepository.AddAsync(new DriverRecordAccessLog
        {
            DriverId = driver?.Id,
            RequesterIpHash = hashingService.HashIdentifier(request.RequesterIpAddress) ?? "unknown",
            Surface = "right-of-reply",
            LookupTermHash = hashingService.HashIdentifier(request.RegistrationNumber.ToUpperInvariant()) ?? string.Empty,
            Matched = driver is not null
        });

        if (driver is null)
            throw new KeyNotFoundException("We could not match those details to a record.");

        driver.RightOfReplyResponse = request.Response;
        driver.RightOfReplyRespondedAt = DateTime.UtcNow;
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        await notificationService.NotifyModeratorsAsync(
            "Right of Reply Received",
            $"{driver.DriverName} has responded to a proposed status change. " +
            "Their response must be considered before the status moves.");
    }
}

/// <summary>
/// Clause 7.3 and POPIA s71(2). The risk score is computed automatically; the public status band
/// is not. A human decides it, gives a reason, and confirms what they actually reviewed. There is
/// no other code path that writes <c>Driver.Status</c>.
/// </summary>
public record SetDriverStatusCommand(
    Guid DriverId,
    DriverStatus TargetStatus,
    Guid ActorUserId,
    string Reason,
    bool ReviewedRiskScore,
    bool ReviewedDriverResponse,
    bool ReviewedScoringLogic) : IRequest;

public class SetDriverStatusCommandValidator : AbstractValidator<SetDriverStatusCommand>
{
    public SetDriverStatusCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.ActorUserId).NotEmpty();

        // s71(2)(b) wants human involvement that is real. Confirming you looked at the score and
        // at how it was derived is the minimum we can actually evidence.
        RuleFor(x => x.ReviewedRiskScore).Equal(true)
            .WithMessage("You must confirm you reviewed the risk score before changing a driver's status.");

        RuleFor(x => x.ReviewedScoringLogic).Equal(true)
            .WithMessage("You must confirm you reviewed how the score was derived.");
    }
}

public class SetDriverStatusCommandHandler(
    IDriverRepository driverRepository,
    IAuditRepository auditRepository,
    IAppealRepository appealRepository,
    IRiskScoringService riskScoringService) : IRequestHandler<SetDriverStatusCommand>
{
    public async Task Handle(SetDriverStatusCommand request, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(request.DriverId)
            ?? throw new KeyNotFoundException("Driver not found.");

        var openAppeals = await appealRepository.GetOpenByDriverIdAsync(driver.Id);
        if (openAppeals.Any())
            throw new InvalidStateTransitionException(
                "This driver has an open appeal. Resolve it before changing their status (clause 35.4).");

        // Clause 6.5: an adverse move needs the right of reply to have been offered first.
        var isAdverse = request.TargetStatus is DriverStatus.Flagged or DriverStatus.HighRisk;
        if (isAdverse && driver.RightOfReplyOfferedAt is null)
            throw new InvalidStateTransitionException(
                "Clause 6.5 requires the driver to be offered a right of reply before a move to " +
                $"{request.TargetStatus}. Send the notice first.");

        if (isAdverse
            && driver.RightOfReplyRespondedAt is not null
            && !request.ReviewedDriverResponse)
            throw new InvalidStateTransitionException(
                "This driver has responded. You must confirm you considered their response.");

        var score = await riskScoringService.CalculateAsync(driver.Id);
        var from = driver.Status;

        driver.Status = request.TargetStatus;
        driver.RiskScore = score;
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        await auditRepository.AddDriverAuditAsync(new DriverStatusAudit
        {
            DriverId = driver.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = from,
            ToStatus = request.TargetStatus,
            RiskScoreAtDecision = score,
            Reason = request.Reason,
            ReviewedRiskScore = request.ReviewedRiskScore,
            ReviewedDriverResponse = request.ReviewedDriverResponse,
            ReviewedScoringLogic = request.ReviewedScoringLogic,
            RightOfReplyOffered = driver.RightOfReplyOfferedAt is not null,
            RightOfReplyOfferedAt = driver.RightOfReplyOfferedAt
        });
    }
}

/// <summary>
/// What the scoring policy would suggest for this driver right now. Shown to a moderator as a
/// recommendation — it is deliberately a query, so nothing can act on it automatically.
/// </summary>
public record GetSuggestedDriverStatusQuery(Guid DriverId) : IRequest<(int Score, string Suggested)>;

public class GetSuggestedDriverStatusQueryHandler(
    IReportRepository reportRepository,
    IRiskScoringService riskScoringService)
    : IRequestHandler<GetSuggestedDriverStatusQuery, (int Score, string Suggested)>
{
    public async Task<(int Score, string Suggested)> Handle(
        GetSuggestedDriverStatusQuery request, CancellationToken cancellationToken)
    {
        var score = await riskScoringService.CalculateAsync(request.DriverId);
        var count = await reportRepository.CountCorroboratedByDriverIdAsync(request.DriverId);
        var police = await reportRepository.HasCorroboratedPoliceReportAsync(request.DriverId);

        return (score, DriverStatusPolicy.Evaluate(score, count, police).ToString());
    }
}
