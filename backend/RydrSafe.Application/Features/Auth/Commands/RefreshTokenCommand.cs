using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken, Guid UserId) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        if (user.RefreshToken is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        if (!passwordHasher.Verify(request.RefreshToken, user.RefreshToken))
            throw new UnauthorizedAccessException("Invalid token.");

        var newRefreshToken = jwtService.GenerateRefreshToken();
        user.RefreshToken = passwordHasher.Hash(newRefreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userRepository.UpdateAsync(user);

        var accessToken = jwtService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, newRefreshToken, user.FullName, user.Email, user.Role.ToString());
    }
}
