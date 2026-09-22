using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.DriverRights.Commands;

/// <summary>
/// Part C clause 35. Lodging an appeal suspends the driver's public status straight away
/// (clause 35.4) — the point of an appeal is that the contested status stops doing damage while
/// it is being reviewed, not after.
/// </summary>
public record CreateAppealCommand(
    string RegistrationNumber,
    string Grounds,
    string Detail,
    string ContactEmail,
    string? ContactPhone,
    string IdentityEvidenceNote,
    string? RequesterIpAddress) : IRequest<Guid>;

public class CreateAppealCommandValidator : AbstractValidator<CreateAppealCommand>
{
    /// <summary>Clause 35.1 — the grounds a driver may appeal on.</summary>
    public static readonly string[] AllowedGrounds =
        ["Inaccuracy", "MistakenIdentity", "DuplicateProfile", "ObjectionToProcessing", "Other"];

    public CreateAppealCommandValidator()
    {
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Grounds).NotEmpty().Must(AllowedGrounds.Contains)
            .WithMessage("Grounds must be one of: " + string.Join(", ", AllowedGrounds) + ".");
        RuleFor(x => x.Detail).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.IdentityEvidenceNote).NotEmpty().MaximumLength(2000)
            .WithMessage("Describe what you can provide to show you are the driver in question.");
    }
}

public class CreateAppealCommandHandler(
    IDriverRepository driverRepository,
    IAppealRepository appealRepository,
    IDriverAccessLogRepository accessLogRepository,
    IHashingService hashingService,
    IRealtimeNotificationService notificationService) : IRequestHandler<CreateAppealCommand, Guid>
{
    private const int MaxAppealsPerDayPerContact = 3;

    public async Task<Guid> Handle(CreateAppealCommand request, CancellationToken cancellationToken)
    {
        var recent = await appealRepository.CountRecentByContactAsync(
            request.ContactEmail, DateTime.UtcNow.AddDays(-1));

        if (recent >= MaxAppealsPerDayPerContact)
            throw new RateLimitedException(
                "You have lodged several appeals today. Contact the Information Officer directly instead.");

        var ipHash = hashingService.HashIdentifier(request.RequesterIpAddress) ?? "unknown";
        var driver = await driverRepository.GetByRegistrationNumberAsync(request.RegistrationNumber);

        await accessLogRepository.AddAsync(new DriverRecordAccessLog
        {
            DriverId = driver?.Id,
            RequesterIpHash = ipHash,
            Surface = "driver-appeal",
            LookupTermHash = hashingService.HashIdentifier(request.RegistrationNumber.ToUpperInvariant()) ?? string.Empty,
            Matched = driver is not null
        });

        // Deliberately the same failure as a bad registration number: an appeal form must not
        // become a way to test which registrations we hold.
        if (driver is null)
            throw new KeyNotFoundException(
                "We could not match those details to a record. Check the registration number, " +
                "or contact the Information Officer.");

        var appeal = new DriverAppeal
        {
            DriverId = driver.Id,
            Grounds = request.Grounds,
            Detail = request.Detail,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            IdentityEvidenceNote = request.IdentityEvidenceNote,
            PublicStatusSuspended = true,
            DueAt = DateTime.UtcNow.AddDays(30)
        };

        await appealRepository.AddAsync(appeal);

        // Clause 35.4 — suppress the public status for as long as the appeal is open.
        driver.PublicStatusSuspended = true;
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        await notificationService.NotifyModeratorsAsync(
            "Driver Appeal Lodged",
            $"An appeal was lodged for {driver.DriverName} on grounds of {request.Grounds}. " +
            $"Public status is suspended until it is resolved. Due {appeal.DueAt:yyyy-MM-dd}.");

        return appeal.Id;
    }
}
