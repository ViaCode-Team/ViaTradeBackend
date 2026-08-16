using System.Text.Json;
using Application.Notifications.Interfaces;
using Application.Notifications.Models;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;

namespace ViaTradeBackend.BackgroundServices;

public sealed class TelegramReminderPublisherService(
	IServiceProvider services,
	INotificationPublisher notificationPublisher,
	NotificationStreamOptions options,
	ILogger<TelegramReminderPublisherService> logger
) : BackgroundService
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	private readonly TimeSpan _publishInterval = TimeSpan.FromSeconds(options.ReminderPublishIntervalSeconds);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Telegram reminder publisher started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await PublishDueRemindersAsync(stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "Unable to publish due Telegram reminders");
			}

			try
			{
				await Task.Delay(_publishInterval, stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
		}

		logger.LogInformation("Telegram reminder publisher stopped");
	}

	private async Task PublishDueRemindersAsync(CancellationToken ct)
	{
		using var scope = services.CreateScope();
		var reminderQueryService = scope.ServiceProvider.GetRequiredService<IReminderQueryService>();
		var reminderCommandService = scope.ServiceProvider.GetRequiredService<IReminderCommandService>();
		var reminders = await reminderQueryService.ListDueAsync(ct);
		logger.LogDebug("Found {ReminderCount} due reminders for Telegram publishing", reminders.Count);

		foreach (var reminder in reminders)
		{
			await PublishReminderAsync(reminder, ct);
			bool isMarked = await reminderCommandService.MarkPublishedAsync(reminder.Id, ct);
			if (isMarked)
				logger.LogInformation(
					"Marked reminder {ReminderId} as published for user {UserId}",
					reminder.Id,
					reminder.UserId
				);
			else
				logger.LogInformation(
					"Reminder {ReminderId} was already delivered or removed before publishing was recorded",
					reminder.Id
				);
		}
	}

	private async Task PublishReminderAsync(ReminderDto reminder, CancellationToken ct)
	{
		var payload = new ReminderNotificationPayload(
			reminder.Id,
			reminder.Text,
			reminder.RemindAt,
			reminder.Instrument?.Symbol
		);
		var notification = new NotificationMessage(
			$"reminder:{reminder.Id}",
			"reminder",
			reminder.UserId,
			reminder.TelegramId,
			JsonSerializer.Serialize(payload, JsonOptions),
			DateTimeOffset.UtcNow
		);
		await notificationPublisher.PublishAsync(notification, ct);

		logger.LogInformation("Published Telegram reminder {ReminderId} for user {UserId}", reminder.Id, reminder.UserId);
	}

	private sealed record ReminderNotificationPayload(int ReminderId, string Text, DateTime RemindAt, string? InstrumentSymbol);
}
