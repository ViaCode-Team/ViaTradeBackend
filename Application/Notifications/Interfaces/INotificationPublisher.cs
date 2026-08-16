using Application.Notifications.Models;

namespace Application.Notifications.Interfaces;

public interface INotificationPublisher
{
	Task PublishAsync(NotificationMessage notification, CancellationToken ct);
}
