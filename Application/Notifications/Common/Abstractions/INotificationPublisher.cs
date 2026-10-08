namespace ViaTrade.Application.Notifications.Common.Abstractions;

public interface INotificationPublisher
{
	Task PublishAsync(NotificationMessage notification, CancellationToken ct);
}
