using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.DriverRights.Queries;

/// <summary>The appeals queue. Moderators and administrators only.</summary>
public record GetAppealsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<AppealDto>>;

public class GetAppealsQueryHandler(
    IAppealRepository appealRepository) : IRequestHandler<GetAppealsQuery, PagedResult<AppealDto>>
{
    public async Task<PagedResult<AppealDto>> Handle(GetAppealsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await appealRepository.GetPagedAsync(request.Page, request.PageSize);
        return new PagedResult<AppealDto>(items.Select(Map).ToList(), total, request.Page, request.PageSize);
    }

    internal static AppealDto Map(DriverAppeal a) => new(
        a.Id,
        a.DriverId,
        a.Driver?.DriverName ?? string.Empty,
        a.Grounds,
        a.Detail,
        a.ContactEmail,
        a.Status.ToString(),
        a.IdentityVerified,
        a.PublicStatusSuspended,
        a.Outcome,
        a.CreatedAt,
        a.DueAt,
        a.ResolvedAt);
}
