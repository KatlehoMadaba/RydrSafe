using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;

namespace RydrSafe.Application.Features.Reports.Commands;

public record CreateReportCommand(
    string DriverName,
    string RegistrationNumber,
    Guid UserId,
    string Category,
    string Severity,
    string Description,
    DateTime IncidentDate,
    bool ReportedToPolice = false,
    string? OfficialReference = null,
    string? DeviceFingerprint = null,
    string? IpAddress = null,
    bool IsAnonymous = false) : IRequest<Guid>;

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
        RuleFor(x => x.IncidentDate).LessThanOrEqualTo(_ => DateTime.UtcNow);
        RuleFor(x => x.OfficialReference).MaximumLength(100);
    }
}

public class CreateReportCommandHandler(
    IReportRepository reportRepository,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IRiskScoringService riskScoringService,
    IRealtimeNotificationService realtimeNotificationService,
    ICategoryAGate categoryAGate,
    IHashingService hashingService) : IRequestHandler<CreateReportCommand, Guid>
{
    public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var category = Enum.Parse<ReportCategory>(request.Category);
        var classification = ReportClassificationPolicy.Classify(category);

        // POPIA s58(2): while prior authorisation is outstanding, Category A reports may not be
        // processed at all. Refusing at submission is the only way to honour that — accepting
        // and storing the report would already be processing.
        if (classification == ReportClassification.CategoryA && !categoryAGate.IsProcessingEnabled)
            throw new CategoryAProcessingDisabledException(categoryAGate.DisabledReason);

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
            Category = category,
            Classification = classification,
            Severity = Enum.Parse<ReportSeverity>(request.Severity),
            Description = request.Description,
            IncidentDate = DateTime.SpecifyKind(request.IncidentDate, DateTimeKind.Utc),
            ReportedToPolice = request.ReportedToPolice,
            OfficialReference = string.IsNullOrWhiteSpace(request.OfficialReference)
                ? null
                : request.OfficialReference.Trim(),
            // Clause 6.3(a) independence signals. Hashed here so the raw address and
            // fingerprint never reach the database.
            SubmissionIpHash = hashingService.HashIdentifier(request.IpAddress),
            SubmissionDeviceHash = hashingService.HashIdentifier(request.DeviceFingerprint),
            // Written now, not at deletion time: once the account row is gone there is nothing
            // left to derive it from, and clause 6.3(a) still has to be able to tell two
            // de-identified reports apart.
            ReporterKeyHash = hashingService.HashIdentifier(request.UserId.ToString()),
            IsAnonymous = request.IsAnonymous,
            Status = ReportStatus.Pending
        };

        await reportRepository.AddAsync(report);

        // Clause 6.2: a new report has no effect on the driver's public standing. The score is
        // recalculated because corroborating this report may have changed nothing, but the
        // status band is only ever moved by a moderator (clause 7.3 / POPIA s71).
        driver.RiskScore = await riskScoringService.CalculateAsync(driver.Id);
        driver.UpdatedAt = DateTime.UtcNow;
        await driverRepository.UpdateAsync(driver);

        if (report.Severity == ReportSeverity.Critical)
        {
            await realtimeNotificationService.NotifyModeratorsAsync(
                "Critical Report Submitted",
                $"A critical report was submitted for driver {driver.DriverName}. Awaiting moderation.");
        }

        return report.Id;
    }
}
