namespace RydrSafe.Application.Common.Interfaces;

public interface IRealtimeNotificationService
{
    Task NotifyModeratorsAsync(string title, string message);
    Task NotifyUserAsync(Guid userId, string title, string message);
}
