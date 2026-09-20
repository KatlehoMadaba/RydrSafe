using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.Auth.Commands;

public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    DateOnly? DateOfBirth,
    IReadOnlyCollection<ConsentAcceptance> Consents,
    string AgreementVersion,
    string Locale,
    string CollectionSurface,
    string? IpAddress) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Clause 3.1. An 18+ rule is RydrSafe's own product decision, not something POPIA imposes:
    /// s34–35 restrict processing children's information, they do not require every platform to
    /// turn under-18s away. We turn them away because reports here concern alleged offences.
    /// </summary>
    public const int MinimumAge = 18;

    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

        RuleFor(x => x.AgreementVersion).NotEmpty()
            .WithMessage("The agreement version presented to the user must be recorded.");

        RuleFor(x => x.DateOfBirth).NotNull()
            .WithMessage("A date of birth is required to confirm you are 18 or older.");

        RuleFor(x => x.DateOfBirth!.Value)
            .Must(BeAtLeastMinimumAge)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage($"You must be {MinimumAge} or older to create a RydrSafe account.");

        // Part D: every required box must be present and ticked. Validating this server-side is
        // the point — a client-side-only check leaves no record of what was agreed.
        RuleFor(x => x.Consents)
            .Must(HasAllRequiredConsents)
            .WithMessage("All required agreements in Part D must be accepted: "
                         + string.Join(", ", ConsentKeys.Required));
    }

    private static bool BeAtLeastMinimumAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dateOfBirth > today) return false;

        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age >= MinimumAge;
    }

    private static bool HasAllRequiredConsents(IReadOnlyCollection<ConsentAcceptance>? consents)
    {
        if (consents is null || consents.Count == 0) return false;

        return ConsentKeys.Required.All(required =>
            consents.Any(c => c.ConsentKey == required && c.Accepted));
    }
}

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IConsentRepository consentRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email already registered.");

        var refreshToken = jwtService.GenerateRefreshToken();
        var acceptedAt = DateTime.UtcNow;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            RefreshToken = passwordHasher.Hash(refreshToken),
            RefreshTokenExpiry = acceptedAt.AddDays(7),
            DateOfBirth = request.DateOfBirth,
            AcceptedAgreementVersion = request.AgreementVersion,
            AcceptedAgreementAt = acceptedAt
        };

        await userRepository.AddAsync(user);

        // One row per checkbox. Stored, not merely validated, so we can show exactly what this
        // account agreed to and when (Part D, and clause 4.3).
        var consents = request.Consents.Select(c => new UserConsent
        {
            UserId = user.Id,
            ConsentKey = c.ConsentKey,
            AgreementVersion = request.AgreementVersion,
            Locale = request.Locale,
            CollectionSurface = request.CollectionSurface,
            Accepted = c.Accepted,
            AcceptedAt = acceptedAt,
            IpAddress = request.IpAddress
        }).ToList();

        await consentRepository.AddRangeAsync(consents);

        var accessToken = jwtService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, refreshToken, user.FullName, user.Email, user.Role.ToString());
    }
}
