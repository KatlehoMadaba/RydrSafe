using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.DriverFollow.Commands;

public record FollowDriverCommand(Guid UserId, Guid DriverId) : IRequest;

public class FollowDriverCommandHandler(IDriverFollowRepository repository)
    : IRequestHandler<FollowDriverCommand>
{
    public async Task Handle(FollowDriverCommand request, CancellationToken cancellationToken)
    {
        var already = await repository.IsFollowingAsync(request.UserId, request.DriverId);
        if (already) return;

        await repository.FollowAsync(new Domain.Entities.DriverFollow
        {
            UserId = request.UserId,
            DriverId = request.DriverId,
        });
    }
}
