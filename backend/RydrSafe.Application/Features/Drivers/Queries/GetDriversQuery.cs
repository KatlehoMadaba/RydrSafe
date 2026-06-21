using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Drivers.Queries;

public record GetDriversQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<DriverListDto>>;

public class GetDriversQueryHandler(
    IDriverRepository driverRepository,
    IReportRepository reportRepository) : IRequestHandler<GetDriversQuery, PagedResult<DriverListDto>>
{
    public async Task<PagedResult<DriverListDto>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await driverRepository.GetAllAsync(request.Page, request.PageSize);
        var total = await driverRepository.CountAsync();

        var dtos = new List<DriverListDto>();
        foreach (var d in drivers)
        {
            var count = await reportRepository.CountByDriverIdAsync(d.Id);
            var reg = d.Vehicles.FirstOrDefault()?.RegistrationNumber;
            dtos.Add(new DriverListDto(d.Id, d.DriverName, d.RiskScore, d.Status.ToString(), count, reg));
        }

        return new PagedResult<DriverListDto>(dtos, total, request.Page, request.PageSize);
    }
}
