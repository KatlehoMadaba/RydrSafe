using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IVerificationHistoryRepository
{
    Task AddAsync(VerificationHistory entry);
    Task<(IEnumerable<VerificationHistory> Items, int Total)> GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<(int Total, int Flagged, int Safe)> GetStatsByUserIdAsync(Guid userId);

    /// <summary>
    /// Clause 25 / 29. Clears the OCR-derived fields (driver name, registration, image hashes)
    /// on rows past their retention date, keeping the row itself as a record that a
    /// verification happened. Returns the number of rows purged.
    /// </summary>
    Task<int> PurgeExpiredOcrDataAsync(DateTime cutoff, int batchSize);
}
