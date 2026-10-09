using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IDriverAccessLogRepository
{
    Task AddAsync(DriverRecordAccessLog entry);

    /// <summary>Anti-enumeration: how many self-check lookups this requester has made since <paramref name="since"/>.</summary>
    Task<int> CountByRequesterSinceAsync(string requesterIpHash, DateTime since);

    Task<int> PurgeOlderThanAsync(DateTime cutoff);
}
