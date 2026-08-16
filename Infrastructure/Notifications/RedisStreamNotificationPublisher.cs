using Application.Notifications.Interfaces;
using Application.Notifications.Models;
using StackExchange.Redis;

namespace Infrastructure.Notifications;

public sealed class RedisStreamNotificationPublisher(
	IConnectionMultiplexer connectionMultiplexer,
	NotificationStreamOptions options
) : INotificationPublisher
{
	private readonly IDatabase _database = connectionMultiplexer.GetDatabase(options.RedisDatabase);

	public Task PublishAsync(NotificationMessage notification, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		NameValueEntry[] values =
		[
			new("notification_id", notification.NotificationId),
			new("type", notification.Type),
			new("user_id", notification.UserId),
			new("chat_id", notification.ChatId),
			new("payload", notification.Payload),
			new("created_at", notification.CreatedAt.ToString("O")),
		];

		return _database.StreamAddAsync(
			options.StreamName,
			values,
			maxLength: options.MaxLength,
			useApproximateMaxLength: true
		);
	}
}
