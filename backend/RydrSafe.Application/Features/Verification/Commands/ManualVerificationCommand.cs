using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;

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
    INotificationRepository notificationRepository)
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

        Domain.Entities.Driver? matchedDriver = null;

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
                });
            }

            return new VerificationResponse(
                request.DriverName, request.RegistrationNumber, request.PhoneNumber,
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

            await verificationHistoryRepository.AddAsync(new VerificationHistory
            {
                UserId = matchUserId,
                DriverId = matchedDriver.Id,
                DriverName = matchedDriver.DriverName,
                RegistrationNumber = request.RegistrationNumber,
                Status = status.ToString(),
                RiskScore = riskScore,
            });
        }

        if (status is DriverStatus.Flagged or DriverStatus.HighRisk)
        {
            await realtimeNotificationService.NotifyModeratorsAsync(
                "Flagged Driver Detected",
                $"Driver {matchedDriver.DriverName} ({request.RegistrationNumber}) matched during manual verification. Risk score: {riskScore}.");

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
