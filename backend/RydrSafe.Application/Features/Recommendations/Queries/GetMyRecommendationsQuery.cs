using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Recommendations.Queries;

public record GetMyRecommendationsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<RecommendationDto>>;

public class GetMyRecommendationsQueryValidator : AbstractValidator<GetMyRecommendationsQuery>
{
    public GetMyRecommendationsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        // Capped so a caller cannot ask for the whole table in one request.
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetMyRecommendationsQueryHandler(
    IRecommendationRepository recommendationRepository)
    : IRequestHandler<GetMyRecommendationsQuery, PagedResult<RecommendationDto>>
{
    public async Task<PagedResult<RecommendationDto>> Handle(
        GetMyRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await recommendationRepository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize);
        // Counted separately rather than from the page: the page only ever holds PageSize rows,
        // so using its length would report the wrong total on every page.
        var total = await recommendationRepository.CountByUserIdAsync(request.UserId);

        var dtos = items.Select(r => new RecommendationDto(
            r.Id, r.UserId, r.Category.ToString(), r.Subject, r.Message, r.Status.ToString(), r.CreatedAt));

        return new PagedResult<RecommendationDto>(dtos, total, request.Page, request.PageSize);
    }
}
