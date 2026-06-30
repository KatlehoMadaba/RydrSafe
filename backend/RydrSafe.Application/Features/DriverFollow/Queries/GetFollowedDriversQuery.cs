using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.DriverFollow.Queries;

public record GetFollowedDriversQuery(Guid UserId) : IRequest<IEnumerable<DriverListDto>>;

public class GetFollowedDriversQueryHandler(
    IDriverFollowRepository driverFollowRepository,
    IReportRepository reportRepository)
    : IRequestHandler<GetFollowedDriversQuery, IEnumerable<DriverListDto>>
{
    public async Task<IEnumerable<DriverListDto>> Handle(GetFollowedDriversQuery request, CancellationToken cancellationToken)
    {
        var follows = await driverFollowRepository.GetFollowedDriversByUserIdAsync(request.UserId);

        var dtos = new List<DriverListDto>();
        foreach (var f in follows)
        {
            var count = await reportRepository.CountByDriverIdAsync(f.Driver.Id);
            var reg = f.Driver.Vehicles.FirstOrDefault()?.RegistrationNumber;
            dtos.Add(new DriverListDto(f.Driver.Id, f.Driver.DriverName, f.Driver.RiskScore, f.Driver.Status.ToString(), count, reg));
        }

        return dtos;
    }
}
