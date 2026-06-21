using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant())
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var refreshToken = jwtService.GenerateRefreshToken();
        user.RefreshToken = passwordHasher.Hash(refreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userRepository.UpdateAsync(user);

        var accessToken = jwtService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, refreshToken, user.FullName, user.Email, user.Role.ToString());
    }
}
