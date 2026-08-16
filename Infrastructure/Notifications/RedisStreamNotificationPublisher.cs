using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTrade.Application.Notifications.Interfaces;
using ViaTrade.Application.Notifications.Models;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Infrastructure.Notifications;

public sealed class RedisStreamNotificationPublisher(
	IConnectionMultiplexer connectionMultiplexer,
	IOptions<NotificationStreamSettings> options
) : INotificationPublisher
{
	private readonly NotificationStreamSettings _options = options.Value;
	private readonly IDatabase _database = connectionMultiplexer.GetDatabase(options.Value.RedisDatabase);

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
			_options.StreamName,
			values,
			maxLength: _options.MaxLength,
			useApproximateMaxLength: true
		);
	}
}
