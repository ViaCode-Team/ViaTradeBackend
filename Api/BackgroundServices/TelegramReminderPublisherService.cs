using System.Text.Json;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Notifications.Common;
using ViaTrade.Application.Notifications.Common.Abstractions;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Reminders.MarkPublished;
using ViaTrade.Configuration.Options;

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
		var dueHandler = scope.ServiceProvider.GetRequiredService<
			IQueryHandler<ListDueRemindersQuery, IReadOnlyList<DueReminderResult>>
		>();
		var publishHandler = scope.ServiceProvider.GetRequiredService<
			ICommandHandler<MarkReminderPublishedCommand, PublishReminderResult>
		>();
		var query = new ListDueRemindersQuery(options.Value.ReminderPublishBatchSize);
		var reminders = await dueHandler.HandleAsync(query, ct);

		logger.LogDebug("Found {ReminderCount} due reminders for Telegram publishing", reminders.Count);

		foreach (var reminder in reminders)
		{
			try
			{
				await PublishReminderAsync(reminder, ct);
				var command = new MarkReminderPublishedCommand(reminder.UserId, reminder.Id);
				var result = await publishHandler.HandleAsync(command, ct);
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
		string? InstrumentSymbol
	);
}
