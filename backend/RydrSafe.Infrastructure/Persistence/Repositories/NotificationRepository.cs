using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Infrastructure.Persistence.Repositories;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId) =>
        await db.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();

    public async Task<Notification?> GetByIdAsync(Guid id) =>
        await db.Notifications.FindAsync(id);

    public async Task AddAsync(Notification notification)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync();
    }
}
