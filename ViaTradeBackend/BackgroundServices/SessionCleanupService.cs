using Application.Auth.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ViaTradeBackend.BackgroundServices;

public class SessionCleanupService(
	IServiceProvider services,
	ILogger<SessionCleanupService> logger,
	IOptions<AuthCookieOptions> options
) : BackgroundService
{
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);
	private readonly int _sessionLifetimeDays = options.Value.RefreshTokenExpiryDays;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("SessionCleanupService started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(_cleanupInterval, stoppingToken);

				using var scope = services.CreateScope();

				var sessionRepo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

				var threshold = DateTime.UtcNow.AddDays(-_sessionLifetimeDays);
				logger.LogInformation("Starting cleanup: sessions older than {Threshold}", threshold);

				var deletedCount = await sessionRepo.CleanupExpiredSessionsAsync(threshold);

				logger.LogInformation("Cleanup finished: removed {Count} expired sessions", deletedCount);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error during cleanup cycle");
			}
		}
	}
}
