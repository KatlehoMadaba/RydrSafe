using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Verification.Queries;

public record GetVerificationStatsQuery(Guid UserId) : IRequest<VerificationStatsDto>;

public class GetVerificationStatsQueryHandler(
    IVerificationHistoryRepository repository)
    : IRequestHandler<GetVerificationStatsQuery, VerificationStatsDto>
{
    public async Task<VerificationStatsDto> Handle(
        GetVerificationStatsQuery request, CancellationToken cancellationToken)
    {
        var (total, flagged, safe) = await repository.GetStatsByUserIdAsync(request.UserId);
        return new VerificationStatsDto(total, flagged, safe);
    }
}
