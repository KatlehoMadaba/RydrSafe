using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Verification.Commands;

public record ManualVerificationCommand(
    string? RegistrationNumber,
    string? DriverName,
    string? PhoneNumber,
    Guid? UserId) : IRequest<VerificationResponse>;

public class ManualVerificationCommandHandler(
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IReportRepository reportRepository,
    IRiskScoringService riskScoringService,
    IRealtimeNotificationService realtimeNotificationService,
    IVerificationHistoryRepository verificationHistoryRepository,
    IDriverFollowRepository driverFollowRepository,
    INotificationRepository notificationRepository,
    IRetentionPolicy retentionPolicy)
    : IRequestHandler<ManualVerificationCommand, VerificationResponse>
{
    public async Task<VerificationResponse> Handle(ManualVerificationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RegistrationNumber) &&
            string.IsNullOrWhiteSpace(request.DriverName) &&
            string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            throw new ArgumentException("At least one of registration number, driver name, or phone number must be provided.");
        }

        Driver? matchedDriver = null;

        if (!string.IsNullOrWhiteSpace(request.RegistrationNumber))
        {
            var vehicle = await vehicleRepository.GetByRegistrationNumberAsync(request.RegistrationNumber);
            if (vehicle is not null)
                matchedDriver = await driverRepository.GetByIdAsync(vehicle.DriverId);
        }

        if (matchedDriver is null && !string.IsNullOrWhiteSpace(request.PhoneNumber))
            matchedDriver = await driverRepository.GetByPhoneNumberAsync(request.PhoneNumber);

        if (matchedDriver is null && !string.IsNullOrWhiteSpace(request.DriverName))
        {
            var candidates = await driverRepository.GetByNameFuzzyAsync(request.DriverName);
            matchedDriver = candidates.FirstOrDefault();
        }

        if (matchedDriver is null)
        {
            if (request.UserId is Guid noMatchUserId)
            {
                await verificationHistoryRepository.AddAsync(new VerificationHistory
                {
                    UserId = noMatchUserId,
                    DriverName = request.DriverName,
                    RegistrationNumber = request.RegistrationNumber,
                    Status = "Safe",
                    RiskScore = 0,
                    OcrDataRetainedUntil = retentionPolicy.VerificationOcrExpiry(DateTime.UtcNow)
                });
            }

            return new VerificationResponse(
                request.DriverName, request.RegistrationNumber, request.PhoneNumber,
                "Safe", 0, 0, false, null);
        }

        var reportCount = await reportRepository.CountCorroboratedByDriverIdAsync(matchedDriver.Id);
        var riskScore = await riskScoringService.CalculateAsync(matchedDriver.Id);

        // Clause 7.3. A verification is a read. It reports the status a moderator set — it does
        // not evaluate, and it does not write one. Previously this recomputed the status from the
        // scoring policy and saved it, which meant looking a driver up could change their
        // standing with no human in the loop, contrary to POPIA s71(2).
        var status = matchedDriver.PublicStatus;

        if (request.UserId is Guid matchUserId)
        {
            await verificationHistoryRepository.AddAsync(new VerificationHistory
            {
                UserId = matchUserId,
                DriverId = matchedDriver.Id,
                DriverName = matchedDriver.DriverName,
                RegistrationNumber = request.RegistrationNumber,
                Status = status.ToString(),
                RiskScore = riskScore,
                OcrDataRetainedUntil = retentionPolicy.VerificationOcrExpiry(DateTime.UtcNow)
            });
        }

        if (status is DriverStatus.Flagged or DriverStatus.HighRisk)
        {
            try
            {
                await realtimeNotificationService.NotifyModeratorsAsync(
                    "Flagged Driver Matched",
                    $"Driver {matchedDriver.DriverName} matched during manual verification. Risk score: {riskScore}.");

                var followers = await driverFollowRepository.GetFollowersByDriverIdAsync(matchedDriver.Id);
                var label = status == DriverStatus.HighRisk ? "High Risk" : "Flagged";

                var title = $"Driver Alert: {matchedDriver.DriverName}";
                var message = $"A driver you are following ({matchedDriver.DriverName}) is currently marked as {label}.";

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

        return new VerificationResponse(
            matchedDriver.DriverName,
            request.RegistrationNumber,
            matchedDriver.PhoneNumber,
            status.ToString(),
            riskScore,
            reportCount,
            true,
            matchedDriver.Id);
    }
}
