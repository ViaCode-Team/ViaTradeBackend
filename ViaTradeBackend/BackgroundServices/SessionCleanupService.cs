using Application.Interfaces.Repositories.Redis;
using Domain.Models.ConfigOptions;
using Microsoft.Extensions.Options;

namespace ViaTradeBackend.BackgroundServices;

public class SessionCleanupService(
	IServiceProvider services,
	ILogger<SessionCleanupService> logger,
	IOptions<AuthCookieOptions> options) : BackgroundService
{
	private readonly IServiceProvider _services = services;
	private readonly ILogger<SessionCleanupService> _logger = logger;
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);
	private readonly int _sessionLifetimeDays = options.Value.RefreshTokenExpiryDays;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("SessionCleanupService started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(_cleanupInterval, stoppingToken);

				using var scope = _services.CreateScope();

				var sessionRepo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

				var threshold = DateTime.UtcNow.AddDays(-_sessionLifetimeDays);
				_logger.LogInformation("Starting cleanup: sessions older than {Threshold}", threshold);

				var deletedCount = await sessionRepo.CleanupExpiredSessionsAsync(threshold);

				_logger.LogInformation("Cleanup finished: removed {Count} expired sessions", deletedCount);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during cleanup cycle");
			}
		}
	}
}
