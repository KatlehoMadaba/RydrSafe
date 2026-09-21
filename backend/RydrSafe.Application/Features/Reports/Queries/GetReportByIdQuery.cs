using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.Reports.Queries;

/// <summary>
/// Returns the full report — including the free-text description and the reporter's identity.
/// Clause 6.4 forbids showing either of those to anyone else, so the caller must be the
/// reporter or a moderator. Being signed in is not enough.
/// </summary>
public record GetReportByIdQuery(Guid Id, Guid RequestingUserId, bool IsModerator) : IRequest<ReportDto?>;

public class GetReportByIdQueryHandler(
    IReportRepository reportRepository) : IRequestHandler<GetReportByIdQuery, ReportDto?>
{
    public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await reportRepository.GetByIdAsync(request.Id);
        if (r is null) return null;

        if (!request.IsModerator && r.UserId != request.RequestingUserId)
            throw new ForbiddenException("You may only view a report you submitted.");

        return Map(r);
    }

    /// <summary>Shared by the moderator queries. Never reachable from a public route.</summary>
    internal static ReportDto Map(Report r) => new(
        r.Id,
        r.DriverId,
        r.Driver?.DriverName ?? string.Empty,
        r.UserId,
        // Clause 29.2 leaves the report standing after its reporter closes their account, so
        // there is genuinely no name to show — say which it is rather than render a blank.
        r.User?.FullName ?? (r.UserId is null ? "Deleted account" : string.Empty),
        r.IsAnonymous,
        r.Category.ToString(),
        r.Classification.ToString(),
        r.Severity.ToString(),
        r.Description,
        r.IncidentDate,
        r.IncidentDatePrecision.ToString(),
        r.ReportedToPolice,
        r.Status.ToString(),
        r.CorroborationPath.ToString(),
        r.OfficialReference,
        r.OfficialReferenceVerified,
        r.CorroboratedAt,
        r.CreatedAt);
}
