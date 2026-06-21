using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.Auth.Commands;

public record RegisterCommand(string FullName, string Email, string Password) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email already registered.");

        var refreshToken = jwtService.GenerateRefreshToken();
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            RefreshToken = passwordHasher.Hash(refreshToken),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
        };

        await userRepository.AddAsync(user);

        var accessToken = jwtService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, refreshToken, user.FullName, user.Email, user.Role.ToString());
    }
}
