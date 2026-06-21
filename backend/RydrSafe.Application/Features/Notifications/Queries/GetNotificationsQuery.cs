using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Notifications.Queries;

public record GetNotificationsQuery(Guid UserId) : IRequest<IEnumerable<NotificationDto>>;

public class GetNotificationsQueryHandler(
    INotificationRepository notificationRepository) : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationDto>>
{
    public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await notificationRepository.GetByUserIdAsync(request.UserId);
        return notifications.Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt));
    }
}
