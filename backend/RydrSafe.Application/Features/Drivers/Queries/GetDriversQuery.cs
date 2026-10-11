using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Drivers.Queries;

/// <param name="Search">
/// Optional. Matches driver name, registration number or phone. Null or blank returns the
/// unfiltered list, so the moderator table uses one path whether or not the box has anything
/// in it.
/// </param>
public record GetDriversQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<DriverListDto>>;

public class GetDriversQueryHandler(
    IDriverRepository driverRepository,
    IReportRepository reportRepository) : IRequestHandler<GetDriversQuery, PagedResult<DriverListDto>>
{
    public async Task<PagedResult<DriverListDto>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await driverRepository.GetAllAsync(request.Page, request.PageSize, request.Search);

        // Counted with the same filter: an unfiltered total against a filtered page would tell
        // the client there are more pages of results than actually matched.
        var total = await driverRepository.CountAsync(request.Search);

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
