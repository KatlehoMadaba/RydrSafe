using MediatR;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Application.Features.DriverFollow.Commands;

public record UnfollowDriverCommand(Guid UserId, Guid DriverId) : IRequest;

public class UnfollowDriverCommandHandler(IDriverFollowRepository repository)
    : IRequestHandler<UnfollowDriverCommand>
{
    public async Task Handle(UnfollowDriverCommand request, CancellationToken cancellationToken) =>
        await repository.UnfollowAsync(request.UserId, request.DriverId);
}
