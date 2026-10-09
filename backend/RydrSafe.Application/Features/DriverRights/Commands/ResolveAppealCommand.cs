using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.DriverRights.Commands;

/// <summary>
/// Clause 35.3. An appeal is decided by a moderator who was not involved in the decision being
/// appealed, and the outcome is written to the driver audit trail like any other status change.
/// </summary>
public record ResolveAppealCommand(
    Guid AppealId,
    AppealStatus Status,
    string Outcome,
    Guid ActorUserId,
    bool ReviewedRiskScore,
    bool ReviewedDriverResponse,
    bool ReviewedScoringLogic) : IRequest;

public class ResolveAppealCommandValidator : AbstractValidator<ResolveAppealCommand>
{
    public ResolveAppealCommandValidator()
    {
        RuleFor(x => x.Outcome).NotEmpty().MinimumLength(10).MaximumLength(4000);
        RuleFor(x => x.ActorUserId).NotEmpty();

        RuleFor(x => x.Status)
            .Must(s => s is AppealStatus.Upheld or AppealStatus.Dismissed or AppealStatus.UnderReview)
            .WithMessage("An appeal may be moved to UnderReview, Upheld or Dismissed.");

        RuleFor(x => x.ReviewedDriverResponse).Equal(true)
            .When(x => x.Status is AppealStatus.Upheld or AppealStatus.Dismissed)
            .WithMessage("You must confirm you considered the driver's submission before deciding an appeal.");
    }
}

public class ResolveAppealCommandHandler(
    IAppealRepository appealRepository,
    IDriverRepository driverRepository,
    IAuditRepository auditRepository,
    IRiskScoringService riskScoringService) : IRequestHandler<ResolveAppealCommand>
{
    public async Task Handle(ResolveAppealCommand request, CancellationToken cancellationToken)
    {
        var appeal = await appealRepository.GetByIdAsync(request.AppealId)
            ?? throw new KeyNotFoundException("Appeal not found.");

        // Clause 35.3 — independence. The moderator whose decision is under appeal cannot be
        // the one who decides it.
        var priorAudits = await auditRepository.GetDriverAuditsAsync(appeal.DriverId);
        var decidedByThisActor = priorAudits
            .Where(a => a.CreatedAt < appeal.CreatedAt)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        if (decidedByThisActor is not null && decidedByThisActor.ActorUserId == request.ActorUserId)
            throw new ForbiddenException(
                "You made the decision under appeal. Clause 35.3 requires a different moderator to review it.");

        var driver = await driverRepository.GetByIdAsync(appeal.DriverId)
            ?? throw new KeyNotFoundException("Driver not found.");

        appeal.Status = request.Status;
        appeal.Outcome = request.Outcome;
        appeal.AssignedTo = request.ActorUserId;

        if (request.Status is AppealStatus.Upheld or AppealStatus.Dismissed)
        {
            appeal.ResolvedBy = request.ActorUserId;
            appeal.ResolvedAt = DateTime.UtcNow;
            appeal.PublicStatusSuspended = false;
        }

        await appealRepository.UpdateAsync(appeal);

        // The suspension lifts only when no other appeal is still open for this driver.
        var stillOpen = await appealRepository.GetOpenByDriverIdAsync(driver.Id);
        var suspended = stillOpen.Any(a => a.Id != appeal.Id);

        var fromStatus = driver.Status;
        var scoreAtDecision = await riskScoringService.CalculateAsync(driver.Id);

        if (request.Status == AppealStatus.Upheld)
        {
            // An upheld appeal means the record was wrong. Reset the public standing and let a
            // fresh corroborated report rebuild it if one ever comes.
            driver.Status = DriverStatus.Safe;
            driver.RiskScore = 0;
        }
        else if (request.Status == AppealStatus.Dismissed)
        {
            driver.RiskScore = scoreAtDecision;
        }

        driver.PublicStatusSuspended = suspended;
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        await auditRepository.AddDriverAuditAsync(new DriverStatusAudit
        {
            DriverId = driver.Id,
            ActorUserId = request.ActorUserId,
            FromStatus = fromStatus,
            ToStatus = driver.Status,
            RiskScoreAtDecision = scoreAtDecision,
            Reason = $"Appeal {request.Status}. {request.Outcome}",
            ReviewedRiskScore = request.ReviewedRiskScore,
            ReviewedDriverResponse = request.ReviewedDriverResponse,
            ReviewedScoringLogic = request.ReviewedScoringLogic,
            RightOfReplyOffered = driver.RightOfReplyOfferedAt is not null,
            RightOfReplyOfferedAt = driver.RightOfReplyOfferedAt,
            RelatedAppealId = appeal.Id
        });
    }
}
