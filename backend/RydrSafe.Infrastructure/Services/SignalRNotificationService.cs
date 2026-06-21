using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Infrastructure.Persistence;

namespace RydrSafe.Infrastructure.Services;

public class SignalRNotificationService(
    IHubContext<NotificationHub> hubContext,
    AppDbContext db) : IRealtimeNotificationService
{
    public async Task NotifyModeratorsAsync(string title, string message)
    {
        var moderators = await db.Users
            .Where(u => u.Role == UserRole.Moderator || u.Role == UserRole.Admin)
            .ToListAsync();

        foreach (var mod in moderators)
        {
            var notification = new Notification
            {
                UserId = mod.Id,
                Title = title,
                Message = message
            };
            db.Notifications.Add(notification);
        }
        await db.SaveChangesAsync();

        await hubContext.Clients.Group("Moderators").SendAsync("ReceiveNotification", new
        {
            title,
            message,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task NotifyUserAsync(Guid userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message
        };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
        {
            title,
            message,
            createdAt = DateTime.UtcNow
        });
    }
}

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role is "Moderator" or "Admin")
            await Groups.AddToGroupAsync(Context.ConnectionId, "Moderators");

        await base.OnConnectedAsync();
    }
}
