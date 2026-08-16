namespace Application.Notifications.Models;

public sealed class NotificationStreamOptions
{
	public int RedisDatabase { get; init; } = 1;

	public string StreamName { get; init; } = "telegram:notifications";

	public int MaxLength { get; init; } = 100_000;

	public int ReminderPublishIntervalSeconds { get; init; } = 30;
}
