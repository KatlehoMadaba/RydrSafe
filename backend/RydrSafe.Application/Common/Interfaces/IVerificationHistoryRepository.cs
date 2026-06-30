using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IVerificationHistoryRepository
{
    Task AddAsync(VerificationHistory entry);
    Task<(IEnumerable<VerificationHistory> Items, int Total)> GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<(int Total, int Flagged, int Safe)> GetStatsByUserIdAsync(Guid userId);
}
