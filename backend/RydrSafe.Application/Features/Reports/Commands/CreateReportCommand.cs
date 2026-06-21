using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Reports.Commands;

public record CreateReportCommand(
    Guid DriverId,
    Guid UserId,
    string Category,
    string Severity,
    string Description,
    DateTime IncidentDate) : IRequest<Guid>;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().Must(c => Enum.TryParse<ReportCategory>(c, out _))
            .WithMessage("Invalid category.");
        RuleFor(x => x.Severity).NotEmpty().Must(s => Enum.TryParse<ReportSeverity>(s, out _))
            .WithMessage("Invalid severity.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.IncidentDate).LessThanOrEqualTo(DateTime.UtcNow);
    }
}

public class CreateReportCommandHandler(
    IReportRepository reportRepository,
    IDriverRepository driverRepository,
    IRiskScoringService riskScoringService,
    IRealtimeNotificationService realtimeNotificationService) : IRequestHandler<CreateReportCommand, Guid>
{
    public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(request.DriverId)
            ?? throw new KeyNotFoundException("Driver not found.");

        var report = new Report
        {
            DriverId = request.DriverId,
            UserId = request.UserId,
            Category = Enum.Parse<ReportCategory>(request.Category),
            Severity = Enum.Parse<ReportSeverity>(request.Severity),
            Description = request.Description,
            IncidentDate = request.IncidentDate
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
