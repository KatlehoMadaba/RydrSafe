using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Drivers.Queries;

public record GetFlaggedDriversCountQuery : IRequest<int>;

public class GetFlaggedDriversCountQueryHandler(IDriverRepository driverRepository)
    : IRequestHandler<GetFlaggedDriversCountQuery, int>
{
    public Task<int> Handle(GetFlaggedDriversCountQuery request, CancellationToken cancellationToken) =>
        driverRepository.CountByStatusAsync(DriverStatus.Flagged, DriverStatus.HighRisk);
}
