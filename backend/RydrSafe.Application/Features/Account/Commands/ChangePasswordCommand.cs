using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Account.Commands;

/// <summary>
/// An in-app password change. Returns a fresh token pair: the change invalidates the stored
/// refresh token, which signs out every other session, and the caller would otherwise sign
/// themselves out by changing their own password.
/// </summary>
public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<AuthResponse>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .WithMessage("Your new password must be at least 8 characters.");
        RuleFor(x => x.NewPassword).NotEqual(x => x.CurrentPassword)
            .WithMessage("Your new password must be different from your current one.");
    }
}

public class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher) : IRequestHandler<ChangePasswordCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Password shape is enforced by ChangePasswordCommandValidator, which ValidationBehavior
        // runs before this handler is reached. What is left here is the part a validator cannot
        // do: checking the supplied password against the stored hash.
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        // A logged-in session is not proof that the account holder is the one at the keyboard.
        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new CredentialConfirmationException("Your current password is incorrect.");

        var refreshToken = jwtService.GenerateRefreshToken();

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.RefreshToken = passwordHasher.Hash(refreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userRepository.UpdateAsync(user);

        var accessToken = jwtService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, refreshToken, user.FullName, user.Email, user.Role.ToString());
    }
}
