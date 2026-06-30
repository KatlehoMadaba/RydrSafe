using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IDriverFollowRepository
{
    Task<bool> IsFollowingAsync(Guid userId, Guid driverId);
    Task FollowAsync(DriverFollow follow);
    Task UnfollowAsync(Guid userId, Guid driverId);
    Task<IEnumerable<DriverFollow>> GetFollowersByDriverIdAsync(Guid driverId);
    Task<IEnumerable<DriverFollow>> GetFollowedDriversByUserIdAsync(Guid userId);
}
