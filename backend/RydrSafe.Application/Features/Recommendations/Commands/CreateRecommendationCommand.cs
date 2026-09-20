using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Application.Features.Recommendations.Commands;

public record CreateRecommendationCommand(
    Guid UserId,
    string Category,
    string Subject,
    string Message) : IRequest<RecommendationDto>;

public class CreateRecommendationCommandValidator : AbstractValidator<CreateRecommendationCommand>
{
    public CreateRecommendationCommandValidator()
    {
        // Bounds match the client-side schema so a valid form never fails server-side.
        RuleFor(x => x.Category).NotEmpty()
            .Must(c => Enum.TryParse<RecommendationCategory>(c, out _))
            .WithMessage("Invalid category.");
        RuleFor(x => x.Subject).NotEmpty().MinimumLength(5).MaximumLength(120);
        RuleFor(x => x.Message).NotEmpty().MinimumLength(20).MaximumLength(2000);
    }
}

public class CreateRecommendationCommandHandler(
    IRecommendationRepository recommendationRepository)
    : IRequestHandler<CreateRecommendationCommand, RecommendationDto>
{
    public async Task<RecommendationDto> Handle(CreateRecommendationCommand request, CancellationToken cancellationToken)
    {
        var recommendation = new Recommendation
        {
            UserId = request.UserId,
            Category = Enum.Parse<RecommendationCategory>(request.Category),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = RecommendationStatus.Pending
        };

        await recommendationRepository.AddAsync(recommendation);

        return new RecommendationDto(
            recommendation.Id,
            recommendation.UserId,
            recommendation.Category.ToString(),
            recommendation.Subject,
            recommendation.Message,
            recommendation.Status.ToString(),
            recommendation.CreatedAt);
    }
}
