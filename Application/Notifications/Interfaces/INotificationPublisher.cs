using ViaTrade.Application.Notifications.Models;

namespace ViaTrade.Application.Notifications.Interfaces;

public interface INotificationPublisher
{
	Task PublishAsync(NotificationMessage notification, CancellationToken ct);
}
