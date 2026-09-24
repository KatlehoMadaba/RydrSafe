namespace RydrSafe.Application.Common.Interfaces;

public interface IOcrService
{
    /// <summary>
    /// Reads text off one screenshot. The image is held in memory for the duration of the call
    /// and never written anywhere; what comes back is the extracted fields plus a hash, which is
    /// the only trace that survives (clause 25).
    /// </summary>
    Task<OcrExtraction> ExtractAsync(Stream imageStream, CancellationToken cancellationToken = default);
}

public record OcrResult(
    string? DriverName,
    string? RegistrationNumber,
    string? PhoneNumber,
    string? VehicleMake,
    string? VehicleModel
);

/// <summary>
/// What an OCR pass produces. <paramref name="ImageHash"/> is retained for duplicate detection
/// under clause 25.3; <paramref name="DiscardedAt"/> records when the image bytes were released,
/// so the deletion promise in clause 25.2 is evidenced rather than merely asserted.
/// </summary>
public record OcrExtraction(OcrResult Result, string ImageHash, DateTime DiscardedAt);
