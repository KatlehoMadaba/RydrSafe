using MediatR;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Application.Features.DriverFollow.Queries;

public record GetFollowStatusQuery(Guid UserId, Guid DriverId) : IRequest<bool>;

public class GetFollowStatusQueryHandler(IDriverFollowRepository repository)
    : IRequestHandler<GetFollowStatusQuery, bool>
{
    public async Task<bool> Handle(GetFollowStatusQuery request, CancellationToken cancellationToken) =>
        await repository.IsFollowingAsync(request.UserId, request.DriverId);
}
