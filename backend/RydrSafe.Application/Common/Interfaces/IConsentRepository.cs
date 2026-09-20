using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IConsentRepository
{
    Task AddRangeAsync(IEnumerable<UserConsent> consents);
    Task<IEnumerable<UserConsent>> GetByUserIdAsync(Guid userId);
    Task WithdrawAsync(Guid userId, string consentKey);

    /// <summary>Clause 29 — clears stored IP addresses on consent rows older than the retention period.</summary>
    Task<int> PurgeIpAddressesOlderThanAsync(DateTime cutoff);
}
