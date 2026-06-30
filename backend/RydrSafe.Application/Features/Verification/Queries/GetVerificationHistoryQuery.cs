using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Verification.Queries;

public record GetVerificationHistoryQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<VerificationHistoryDto>>;

public class GetVerificationHistoryQueryHandler(
    IVerificationHistoryRepository repository)
    : IRequestHandler<GetVerificationHistoryQuery, PagedResult<VerificationHistoryDto>>
{
    public async Task<PagedResult<VerificationHistoryDto>> Handle(
        GetVerificationHistoryQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await repository.GetByUserIdAsync(request.UserId, page, pageSize);

        var dtos = items.Select(v => new VerificationHistoryDto(
            v.Id, v.DriverName, v.RegistrationNumber, v.Status, v.RiskScore, v.VerifiedAt));

        return new PagedResult<VerificationHistoryDto>(dtos, total, page, pageSize);
    }
}
