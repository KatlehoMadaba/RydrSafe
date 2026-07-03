using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Verification.Commands;

public record ExtractDriverInfoCommand(
    Stream Image1,
    Stream? Image2,
    Stream? Image3) : IRequest<ExtractDriverInfoResult>;

public record ExtractDriverInfoResult(string? DriverName, string? RegistrationNumber, string? PhoneNumber);

public class ExtractDriverInfoCommandHandler(
    IOcrService ocrService,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository) : IRequestHandler<ExtractDriverInfoCommand, ExtractDriverInfoResult>
{
    public async Task<ExtractDriverInfoResult> Handle(ExtractDriverInfoCommand request, CancellationToken cancellationToken)
    {
        var ocr = await ocrService.ExtractAsync(request.Image1);

        if (request.Image2 is not null)
            ocr = Merge(ocr, await ocrService.ExtractAsync(request.Image2));

        if (request.Image3 is not null)
            ocr = Merge(ocr, await ocrService.ExtractAsync(request.Image3));

        string? driverName = ocr.DriverName;
        string? registrationNumber = ocr.RegistrationNumber;
        string? phoneNumber = ocr.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(ocr.RegistrationNumber))
        {
            var vehicle = await vehicleRepository.GetByRegistrationNumberAsync(ocr.RegistrationNumber);
            if (vehicle is not null)
            {
                var driver = await driverRepository.GetByIdAsync(vehicle.DriverId);
                if (driver is not null)
                {
                    driverName = driver.DriverName;
                    phoneNumber ??= driver.PhoneNumber;
                }
            }
        }

        if (driverName is null && !string.IsNullOrWhiteSpace(ocr.PhoneNumber))
        {
            var driver = await driverRepository.GetByPhoneNumberAsync(ocr.PhoneNumber);
            if (driver is not null)
                driverName = driver.DriverName;
        }

        return new ExtractDriverInfoResult(driverName, registrationNumber, phoneNumber);
    }

    private static OcrResult Merge(OcrResult a, OcrResult b) => new(
        a.DriverName ?? b.DriverName,
        a.RegistrationNumber ?? b.RegistrationNumber,
        a.PhoneNumber ?? b.PhoneNumber,
        a.VehicleMake ?? b.VehicleMake,
        a.VehicleModel ?? b.VehicleModel);
}
