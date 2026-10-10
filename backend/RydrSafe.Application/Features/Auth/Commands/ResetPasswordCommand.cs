using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        // Matches the registration rule; a reset must not be a way to set a weaker password.
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IPasswordHasher passwordHasher,
    IHashingService hashingService) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var hash = hashingService.HashIdentifier(request.Token)!;
        var token = await tokenRepository.GetByHashAsync(hash);

        // One message for every failure mode — unknown, expired, already spent. Distinguishing
        // them would tell an attacker which guessed tokens once existed.
        if (token is null || !token.IsRedeemable(DateTime.UtcNow))
            throw new UnauthorizedAccessException("This reset link is invalid or has expired.");

        var user = token.User;
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

        // Whoever reset the password now owns the account; anyone else holding a live session
        // should not keep it. Clearing the refresh token ends those sessions at next refresh.
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        // Spend this token and every other outstanding one, so a second reset email already
        // sitting in the inbox cannot be redeemed afterwards.
        await tokenRepository.InvalidateAllForUserAsync(user.Id, DateTime.UtcNow);
        await userRepository.UpdateAsync(user);
        await tokenRepository.SaveChangesAsync();
    }
}
