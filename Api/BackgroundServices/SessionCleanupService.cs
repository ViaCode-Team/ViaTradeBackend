using ViaTrade.Application.Auth.Interfaces;

namespace ViaTrade.Api.BackgroundServices;

public class SessionCleanupService(IServiceProvider services, ILogger<SessionCleanupService> logger) : BackgroundService
{
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("SessionCleanupService started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = services.CreateScope();

				var sessionRepo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

				var deletedCount = await sessionRepo.CleanupExpiredSessionsAsync(DateTime.UtcNow);

				if (deletedCount > 0)
					logger.LogInformation("Session cleanup removed {Count} expired session indexes", deletedCount);

				await Task.Delay(_cleanupInterval, stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error during cleanup cycle");
				await Task.Delay(_cleanupInterval, stoppingToken);
			}
		}
	}
}
