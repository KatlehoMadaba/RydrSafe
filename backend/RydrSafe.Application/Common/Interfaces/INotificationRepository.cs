using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
}
