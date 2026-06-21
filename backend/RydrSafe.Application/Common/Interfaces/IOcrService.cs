namespace RydrSafe.Application.Common.Interfaces;

public interface IOcrService
{
    Task<OcrResult> ExtractAsync(Stream imageStream);
}

public record OcrResult(
    string? DriverName,
    string? RegistrationNumber,
    string? PhoneNumber,
    string? VehicleMake,
    string? VehicleModel
);
