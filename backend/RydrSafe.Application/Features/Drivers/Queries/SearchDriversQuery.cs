using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Drivers.Queries;

public record SearchDriversQuery(string Query) : IRequest<IEnumerable<DriverListDto>>;

public class SearchDriversQueryHandler(
    IDriverRepository driverRepository,
    IReportRepository reportRepository) : IRequestHandler<SearchDriversQuery, IEnumerable<DriverListDto>>
{
    public async Task<IEnumerable<DriverListDto>> Handle(SearchDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await driverRepository.SearchAsync(request.Query);
        var result = new List<DriverListDto>();

        foreach (var d in drivers)
        {
            var count = await reportRepository.CountByDriverIdAsync(d.Id);
            var reg = d.Vehicles.FirstOrDefault()?.RegistrationNumber;
            result.Add(new DriverListDto(d.Id, d.DriverName, d.RiskScore, d.Status.ToString(), count, reg));
        }

        return result;
    }
}
