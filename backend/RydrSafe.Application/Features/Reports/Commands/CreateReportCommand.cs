using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Commands;

public record CreateReportCommand(
    string DriverName,
    string RegistrationNumber,
    Guid UserId,
    string Category,
    string Severity,
    string Description,
    DateTime IncidentDate) : IRequest<Guid>;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.DriverName).NotEmpty();
        RuleFor(x => x.RegistrationNumber).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().Must(c => Enum.TryParse<ReportCategory>(c, out _))
            .WithMessage("Invalid category.");
        RuleFor(x => x.Severity).NotEmpty().Must(s => Enum.TryParse<ReportSeverity>(s, out _))
            .WithMessage("Invalid severity.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.IncidentDate).LessThanOrEqualTo(DateTime.Now);
    }
}

public class CreateReportCommandHandler(
    IReportRepository reportRepository,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IRiskScoringService riskScoringService,
    IRealtimeNotificationService realtimeNotificationService) : IRequestHandler<CreateReportCommand, Guid>
{
    public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        // Look up driver by registration number; create if this is the first report about them
        var driver = await driverRepository.GetByRegistrationNumberAsync(request.RegistrationNumber);

        if (driver is null)
        {
            driver = new Driver
            {
                DriverName = request.DriverName,
                Status = DriverStatus.Safe,
                RiskScore = 0,
            };
            await driverRepository.AddAsync(driver);

            var vehicle = new Vehicle
            {
                DriverId = driver.Id,
                RegistrationNumber = request.RegistrationNumber.ToUpper(),
            };
            await vehicleRepository.AddAsync(vehicle);
        }

        var report = new Report
        {
            DriverId = driver.Id,
            UserId = request.UserId,
            Category = Enum.Parse<ReportCategory>(request.Category),
            Severity = Enum.Parse<ReportSeverity>(request.Severity),
            Description = request.Description,
            IncidentDate = DateTime.SpecifyKind(request.IncidentDate, DateTimeKind.Utc)
        };

        await reportRepository.AddAsync(report);

        var newScore = await riskScoringService.CalculateAsync(driver.Id);
        driver.RiskScore = newScore;
        driver.Status = newScore switch
        {
            >= 80 => DriverStatus.HighRisk,
            >= 60 => DriverStatus.Flagged,
            >= 30 => DriverStatus.UnderReview,
            _ => DriverStatus.Safe
        };
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        if (report.Severity == ReportSeverity.Critical)
        {
            await realtimeNotificationService.NotifyModeratorsAsync(
                "Critical Report Submitted",
                $"A critical report was submitted for driver {driver.DriverName}.");
        }

        return report.Id;
    }
}
