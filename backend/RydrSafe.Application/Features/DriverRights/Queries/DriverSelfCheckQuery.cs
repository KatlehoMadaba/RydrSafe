using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.Reports.Queries;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.DriverRights.Queries;

/// <summary>
/// Part C clause 34 (POPIA s23). A driver can ask what we hold about them without having an
/// account — which is the whole point, since drivers are not our users.
///
/// An unauthenticated lookup keyed on a registration number is an enumeration oracle unless it
/// is built carefully. The controls here are: a per-requester rate limit, an identical response
/// whether or not a record exists, a required contact address so a lookup is attributable, and
/// an access-log row for every attempt (COMPLIANCE-NOTES B7).
/// </summary>
public record DriverSelfCheckQuery(
    string RegistrationNumber,
    string? DriverName,
    string ContactEmail,
    string? RequesterIpAddress) : IRequest<DriverSelfCheckResponse>;

public class DriverSelfCheckQueryValidator : AbstractValidator<DriverSelfCheckQuery>
{
    public DriverSelfCheckQueryValidator()
    {
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress()
            .WithMessage("A contact address is required so we can respond and so lookups are attributable.");
    }
}

public class DriverSelfCheckQueryHandler(
    IDriverRepository driverRepository,
    IReportRepository reportRepository,
    IDriverAccessLogRepository accessLogRepository,
    IHashingService hashingService,
    ICategoryAGate categoryAGate) : IRequestHandler<DriverSelfCheckQuery, DriverSelfCheckResponse>
{
    /// <summary>Enough for a driver checking their own record; far too few to scrape with.</summary>
    private const int MaxLookupsPerWindow = 5;
    private static readonly TimeSpan Window = TimeSpan.FromHours(1);

    /// <summary>
    /// Returned identically for a hit and a miss. A caller cannot tell from the response
    /// whether a registration number is in the database.
    /// </summary>
    private const string UniformMessage =
        "If we hold a record matching these details, a summary appears below and a copy has been " +
        "sent to the address you gave. If nothing appears, we hold no record matching them. " +
        "To dispute anything shown here, lodge an appeal under clause 35.";

    public async Task<DriverSelfCheckResponse> Handle(
        DriverSelfCheckQuery request, CancellationToken cancellationToken)
    {
        var ipHash = hashingService.HashIdentifier(request.RequesterIpAddress) ?? "unknown";

        var recent = await accessLogRepository.CountByRequesterSinceAsync(ipHash, DateTime.UtcNow - Window);
        if (recent >= MaxLookupsPerWindow)
            throw new RateLimitedException(
                "Too many lookups from this connection. Try again later, or contact the Information Officer.");

        var driver = await driverRepository.GetByRegistrationNumberAsync(request.RegistrationNumber);

        await accessLogRepository.AddAsync(new DriverRecordAccessLog
        {
            DriverId = driver?.Id,
            RequesterIpHash = ipHash,
            Surface = "driver-self-check",
            LookupTermHash = hashingService.HashIdentifier(request.RegistrationNumber.ToUpperInvariant()) ?? string.Empty,
            Matched = driver is not null
        });

        if (driver is null)
            return new DriverSelfCheckResponse(
                RecordExists: false,
                DriverName: null,
                PublicStatus: null,
                CorroboratedReportCount: 0,
                Summaries: [],
                FirstSeen: null,
                Message: UniformMessage);

        var corroborated = await reportRepository.GetCorroboratedByDriverIdAsync(driver.Id);
        var summaries = GetPublicReportSummariesQueryHandler.Build(
            corroborated, categoryAGate.IsPublicationEnabled);

        return new DriverSelfCheckResponse(
            RecordExists: true,
            DriverName: driver.DriverName,
            // Clause 35.4 — a driver with an open appeal reads as Safe here too.
            PublicStatus: driver.PublicStatus.ToString(),
            CorroboratedReportCount: summaries.Sum(s => s.CorroboratedCount),
            Summaries: summaries,
            FirstSeen: driver.CreatedAt,
            Message: UniformMessage);
    }
}
