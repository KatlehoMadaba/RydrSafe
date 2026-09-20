using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Verification.Commands;

public record UploadVerificationCommand(
    Stream Image1,
    Stream? Image2,
    Stream? Image3,
    Guid? UserId) : IRequest<VerificationResponse>;

public class UploadVerificationCommandHandler(
    IOcrService ocrService,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IReportRepository reportRepository,
    IRiskScoringService riskScoringService,
    IRealtimeNotificationService realtimeNotificationService,
    IVerificationHistoryRepository verificationHistoryRepository,
    IDriverFollowRepository driverFollowRepository,
    INotificationRepository notificationRepository,
    IRetentionPolicy retentionPolicy) : IRequestHandler<UploadVerificationCommand, VerificationResponse>
{
    public async Task<VerificationResponse> Handle(UploadVerificationCommand request, CancellationToken cancellationToken)
    {
        // Each screenshot is a separate paid OCR call against a monthly quota, so stop as soon
        // as we hold enough to identify a driver. These ran in parallel for latency, but that
        // always spent three calls — and quota, not latency, is what runs out. The short-circuit
        // makes the three-image case rare enough that the sequential cost rarely applies.
        //
        // Clause 25: every image we actually read is hashed and discarded by ExtractAsync, and
        // recorded below. An image skipped by the short-circuit is never read at all.
        var streams = new[] { request.Image1, request.Image2, request.Image3 }
            .Where(s => s is not null)
            .Select(s => s!)
            .ToList();

        var extractions = new List<OcrExtraction>();
        OcrResult? merged = null;

        foreach (var stream in streams)
        {
            var extraction = await ocrService.ExtractAsync(stream, cancellationToken);
            extractions.Add(extraction);

            merged = merged is null
                ? extraction.Result
                : MergeOcrResults(merged, extraction.Result);

            if (CanAttemptMatch(merged)) break;
        }

        var ocr = merged!;
        var imageHashes = string.Join(",", extractions.Select(e => e.ImageHash));
        var imagesDiscardedAt = extractions.Max(e => e.DiscardedAt);

        Driver? matchedDriver = null;

        if (!string.IsNullOrWhiteSpace(ocr.RegistrationNumber))
        {
            var vehicle = await vehicleRepository.GetByRegistrationNumberAsync(ocr.RegistrationNumber);
            if (vehicle is not null)
                matchedDriver = await driverRepository.GetByIdAsync(vehicle.DriverId);
        }

        if (matchedDriver is null && !string.IsNullOrWhiteSpace(ocr.PhoneNumber))
            matchedDriver = await driverRepository.GetByPhoneNumberAsync(ocr.PhoneNumber);

        if (matchedDriver is null && !string.IsNullOrWhiteSpace(ocr.DriverName))
        {
            var candidates = await driverRepository.GetByNameFuzzyAsync(ocr.DriverName);
            matchedDriver = candidates.FirstOrDefault();
        }

        if (matchedDriver is null)
        {
            if (request.UserId is Guid noMatchUserId)
                await RecordHistoryAsync(noMatchUserId, null, ocr, "Safe", 0, imageHashes, imagesDiscardedAt);

            return new VerificationResponse(
                ocr.DriverName, ocr.RegistrationNumber, ocr.PhoneNumber,
                "Safe", 0, 0, false, null);
        }

        // Clause 6.2 and 6.4: only corroborated reports are visible or count towards standing.
        var reportCount = await reportRepository.CountCorroboratedByDriverIdAsync(matchedDriver.Id);
        var riskScore = await riskScoringService.CalculateAsync(matchedDriver.Id);

        // Clause 7.3 / 35.4: the status shown is the one a moderator set, suppressed while an
        // appeal is open. A verification never computes or writes a status of its own — doing so
        // would be automated processing changing a person's standing without human involvement.
        var status = matchedDriver.PublicStatus;

        if (request.UserId is Guid matchUserId)
            await RecordHistoryAsync(
                matchUserId, matchedDriver, ocr, status.ToString(), riskScore, imageHashes, imagesDiscardedAt);

        if (status is DriverStatus.Flagged or DriverStatus.HighRisk)
            await NotifyFollowersAsync(matchedDriver, status, riskScore);

        return new VerificationResponse(
            matchedDriver.DriverName,
            ocr.RegistrationNumber,
            matchedDriver.PhoneNumber,
            status.ToString(),
            riskScore,
            reportCount,
            true,
            matchedDriver.Id);
    }

    private async Task RecordHistoryAsync(
        Guid userId, Driver? driver, OcrResult ocr, string status, int riskScore,
        string imageHashes, DateTime imagesDiscardedAt)
    {
        await verificationHistoryRepository.AddAsync(new VerificationHistory
        {
            UserId = userId,
            DriverId = driver?.Id,
            DriverName = driver?.DriverName ?? ocr.DriverName,
            RegistrationNumber = ocr.RegistrationNumber,
            Status = status,
            RiskScore = riskScore,
            // Clause 25: the images are already gone by this point; the hash and the timestamp
            // are what is left, and the retention date is set now so the worker can find it.
            ImageHashes = imageHashes,
            ImagesDiscardedAt = imagesDiscardedAt,
            OcrDataRetainedUntil = retentionPolicy.VerificationOcrExpiry(DateTime.UtcNow)
        });
    }

    private async Task NotifyFollowersAsync(Driver driver, DriverStatus status, int riskScore)
    {
        try
        {
            await realtimeNotificationService.NotifyModeratorsAsync(
                "Flagged Driver Matched",
                $"Driver {driver.DriverName} matched during verification. Risk score: {riskScore}.");

            var followers = await driverFollowRepository.GetFollowersByDriverIdAsync(driver.Id);
            var label = status == DriverStatus.HighRisk ? "High Risk" : "Flagged";

            var title = $"Driver Alert: {driver.DriverName}";
            var message = $"A driver you are following ({driver.DriverName}) is currently marked as {label}.";

            // Clause 6.4 — the alert carries the driver's public standing only. It used to
            // include the OCR-extracted registration number, which put freshly extracted
            // personal information into every follower's notification row.
            foreach (var follow in followers)
            {
                await notificationRepository.AddAsync(new Notification
                {
                    UserId = follow.UserId,
                    Title = title,
                    Message = message,
                });
                await realtimeNotificationService.NotifyUserAsync(follow.UserId, title, message);
            }
        }
        catch
        {
            // A notification failure must not prevent the verification response being returned.
        }
    }
    /// <summary>
    /// Whether we hold enough to look a driver up without scanning more photos.
    /// Registration number and phone number are exact lookups; a name alone is not enough,
    /// because it only reaches the fuzzy match below and extra photos genuinely help there.
    /// Mirrors the lookup order in <see cref="Handle"/> — keep the two in step.
    /// </summary>
    private static bool CanAttemptMatch(OcrResult ocr) =>
        !string.IsNullOrWhiteSpace(ocr.RegistrationNumber)
        || !string.IsNullOrWhiteSpace(ocr.PhoneNumber);

    private static OcrResult MergeOcrResults(OcrResult a, OcrResult b) => new(
        a.DriverName ?? b.DriverName,
        a.RegistrationNumber ?? b.RegistrationNumber,
        a.PhoneNumber ?? b.PhoneNumber,
        a.VehicleMake ?? b.VehicleMake,
        a.VehicleModel ?? b.VehicleModel);
}
