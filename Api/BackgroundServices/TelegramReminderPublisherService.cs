using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Notifications.Common;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Reminders.MarkPublished;
using ViaTrade.Configuration.Options;
using INotificationPublisher = ViaTrade.Application.Notifications.Common.Abstractions.INotificationPublisher;

namespace ViaTrade.Api.BackgroundServices;

public sealed class TelegramReminderPublisherService(
	IServiceProvider services,
	INotificationPublisher notificationPublisher,
	IOptions<NotificationStreamSettings> options,
	ILogger<TelegramReminderPublisherService> logger
) : BackgroundService
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	private readonly TimeSpan _publishInterval = TimeSpan.FromSeconds(options.Value.ReminderPublishIntervalSeconds);

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
		var sender = scope.ServiceProvider.GetRequiredService<ISender>();
		var query = new ListDueRemindersQuery(options.Value.ReminderPublishBatchSize);
		var reminders = await sender.Send(query, ct);

		logger.LogDebug("Found {ReminderCount} due reminders for Telegram publishing", reminders.Count);

		foreach (var reminder in reminders)
		{
			try
			{
				await PublishReminderAsync(reminder, ct);

				var command = new MarkReminderPublishedCommand(reminder.UserId, reminder.Id);
				var result = await sender.Send(command, ct);

				if (result.IsPublished)
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
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exception)
			{
				logger.LogError(
					exception,
					"Failed to publish or mark reminder {ReminderId} for user {UserId}. Skipping to next.",
					reminder.Id,
					reminder.UserId
				);
			}
		}
	}

	private async Task PublishReminderAsync(DueReminderResult reminder, CancellationToken ct)
	{
		var payload = new ReminderNotificationPayload(
			reminder.Id,
			reminder.Text,
			reminder.RemindAt,
			reminder.Instrument?.Ticker
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

		logger.LogInformation(
			"Published Telegram reminder {ReminderId} for user {UserId}",
			reminder.Id,
			reminder.UserId
		);
	}

	private sealed record ReminderNotificationPayload(
		int ReminderId,
		string Text,
		DateTime RemindAt,
		string? InstrumentTicker
	);
}
