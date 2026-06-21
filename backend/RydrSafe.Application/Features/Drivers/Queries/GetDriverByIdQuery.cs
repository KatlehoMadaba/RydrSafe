using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Drivers.Queries;

public record GetDriverByIdQuery(Guid Id) : IRequest<DriverDto?>;

public class GetDriverByIdQueryHandler(
    IDriverRepository driverRepository,
    IReportRepository reportRepository) : IRequestHandler<GetDriverByIdQuery, DriverDto?>
{
    public async Task<DriverDto?> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(request.Id);
        if (driver is null) return null;

        var count = await reportRepository.CountByDriverIdAsync(driver.Id);
        var vehicles = driver.Vehicles.Select(v => new VehicleDto(v.Id, v.RegistrationNumber, v.Make, v.Model, v.Color));

        return new DriverDto(driver.Id, driver.DriverName, driver.PhoneNumber,
            driver.RiskScore, driver.Status.ToString(), count, vehicles, driver.CreatedAt);
    }
}
