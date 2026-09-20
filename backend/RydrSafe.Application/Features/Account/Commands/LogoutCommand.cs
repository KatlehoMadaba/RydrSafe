using MediatR;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Application.Features.Account.Commands;

/// <summary>
/// Clearing local storage only signs a browser out. This clears the stored refresh token as
/// well, so a copy of it taken from that browser cannot be traded for a new session afterwards.
/// </summary>
public record LogoutCommand(Guid UserId) : IRequest;

public class LogoutCommandHandler(IUserRepository userRepository) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        // Signing out an account that is already gone is not an error — the client is trying to
        // clean up, and telling it off achieves nothing.
        if (user is null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await userRepository.UpdateAsync(user);
    }
}
