using Microsoft.Extensions.Options;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.BackgroundServices;

public sealed class ReminderCleanupService(
	IServiceProvider services,
	IOptions<ReminderCleanupSettings> options,
	ILogger<ReminderCleanupService> logger
) : BackgroundService
{
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(options.Value.CleanupIntervalHours);
	private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(options.Value.RetentionDays);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Reminder cleanup service started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await DeleteExpiredRemindersAsync(stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "Unable to delete expired delivered reminders");
			}

			try
			{
				await Task.Delay(_cleanupInterval, stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
		}

		logger.LogInformation("Reminder cleanup service stopped");
	}

	private async Task DeleteExpiredRemindersAsync(CancellationToken ct)
	{
		using var scope = services.CreateScope();
		var reminderCommandService = scope.ServiceProvider.GetRequiredService<IReminderCommandService>();

		var deliveredBefore = DateTime.UtcNow.Subtract(_retentionPeriod);

		int deletedCount = await reminderCommandService.DeleteDeliveredBeforeAsync(deliveredBefore, ct);

		if (deletedCount > 0)
			logger.LogInformation(
				"Deleted {ReminderCount} delivered reminders older than {DeliveredBefore}",
				deletedCount,
				deliveredBefore
			);
	}
}
