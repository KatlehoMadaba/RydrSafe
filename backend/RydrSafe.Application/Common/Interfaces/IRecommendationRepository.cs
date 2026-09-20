using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IRecommendationRepository
{
    Task<Recommendation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Recommendation>> GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(Recommendation recommendation);
}
