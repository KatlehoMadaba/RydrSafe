namespace RydrSafe.Application.DTOs;

public record CreateRecommendationRequest(
    string Category,
    string Subject,
    string Message
);

public record RecommendationDto(
    Guid Id,
    Guid UserId,
    string Category,
    string Subject,
    string Message,
    string Status,
    DateTime CreatedAt
);
