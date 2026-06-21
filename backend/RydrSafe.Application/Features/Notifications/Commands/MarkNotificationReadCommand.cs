using MediatR;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Application.Features.Notifications.Commands;

public record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest;

public class MarkNotificationReadCommandHandler(
    INotificationRepository notificationRepository) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(request.NotificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != request.UserId)
            throw new UnauthorizedAccessException();

        notification.IsRead = true;
        await notificationRepository.UpdateAsync(notification);
    }
}
