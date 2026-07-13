using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;

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
    INotificationRepository notificationRepository) : IRequestHandler<UploadVerificationCommand, VerificationResponse>
{
    public async Task<VerificationResponse> Handle(UploadVerificationCommand request, CancellationToken cancellationToken)
    {
        var ocr = await ocrService.ExtractAsync(request.Image1);

        if (request.Image2 is not null)
        {
            var ocr2 = await ocrService.ExtractAsync(request.Image2);
            ocr = MergeOcrResults(ocr, ocr2);
        }

        if (request.Image3 is not null)
        {
            var ocr3 = await ocrService.ExtractAsync(request.Image3);
            ocr = MergeOcrResults(ocr, ocr3);
        }

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
            {
                await verificationHistoryRepository.AddAsync(new Domain.Entities.VerificationHistory
                {
                    UserId = noMatchUserId,
                    DriverName = ocr.DriverName,
                    RegistrationNumber = ocr.RegistrationNumber,
                    Status = "Safe",
                    RiskScore = 0,
                });
            }

            return new VerificationResponse(
                ocr.DriverName, ocr.RegistrationNumber, ocr.PhoneNumber,
                "Safe", 0, 0, false, null);
        }

        var reportCount = await reportRepository.CountActiveByDriverIdAsync(matchedDriver.Id);
        var riskScore = await riskScoringService.CalculateAsync(matchedDriver.Id);
        var reportedToPolice = await reportRepository.HasPoliceReportAsync(matchedDriver.Id);
        var status = DriverStatusPolicy.Evaluate(riskScore, reportCount, reportedToPolice);

        // Anonymous verification is read-only: the recalculated status is returned to the caller but
        // not persisted, so every stored status change stays attributable to an audited actor
        // (a report, or an authenticated verification recorded in VerificationHistory below).
        if (request.UserId is Guid matchUserId)
        {
            matchedDriver.RiskScore = riskScore;
            matchedDriver.Status = status;
            matchedDriver.UpdatedAt = DateTime.UtcNow;
            await driverRepository.UpdateAsync(matchedDriver);

            await verificationHistoryRepository.AddAsync(new Domain.Entities.VerificationHistory
            {
                UserId = matchUserId,
                DriverId = matchedDriver.Id,
                DriverName = matchedDriver.DriverName,
                RegistrationNumber = ocr.RegistrationNumber,
                Status = status.ToString(),
                RiskScore = riskScore,
            });
        }

        if (status is DriverStatus.Flagged or DriverStatus.HighRisk)
        {
            try
            {
                await realtimeNotificationService.NotifyModeratorsAsync(
                    "Flagged Driver Detected",
                    $"Driver {matchedDriver.DriverName} ({ocr.RegistrationNumber}) matched during verification. Risk score: {riskScore}.");

                var followers = await driverFollowRepository.GetFollowersByDriverIdAsync(matchedDriver.Id);
                var label = status == DriverStatus.HighRisk ? "High Risk" : "Flagged";
                foreach (var follow in followers)
                {
                    var title = $"Driver Alert: {matchedDriver.DriverName}";
                    var message = $"A driver you are following ({matchedDriver.DriverName}) has been marked as {label} with a risk score of {riskScore}.";
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
                // notification failure must not prevent the verification response from being returned
            }
        }

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

    private static OcrResult MergeOcrResults(OcrResult a, OcrResult b) => new(
        a.DriverName ?? b.DriverName,
        a.RegistrationNumber ?? b.RegistrationNumber,
        a.PhoneNumber ?? b.PhoneNumber,
        a.VehicleMake ?? b.VehicleMake,
        a.VehicleModel ?? b.VehicleModel);
}
