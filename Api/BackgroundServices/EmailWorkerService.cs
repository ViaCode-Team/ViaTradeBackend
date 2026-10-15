using Microsoft.Extensions.Options;
using ViaTrade.Application.Email;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.BackgroundServices;

public sealed class EmailWorkerService(
	IServiceScopeFactory scopes,
	IEmailStreamWorker worker,
	IEmailQueue queue,
	IEmailTransport transport,
	IOptions<EmailSettings> options,
	ILogger<EmailWorkerService> logger
) : BackgroundService
{
	private readonly string _consumer = Environment.MachineName + "-" + Guid.NewGuid().ToString("N");
	private int _maintenanceCursor;
	private DateTime _nextMaintenance;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await worker.InitializeAsync(stoppingToken);
				break;
			}
			catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
			{
				logger.LogWarning("Email queue initialization failed: {ExceptionType}", exception.GetType().Name);
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
			}
		}

		await Task.WhenAll(
			RunLoopAsync(false, stoppingToken),
			RunLoopAsync(true, stoppingToken),
			RunLoopAsync(null, stoppingToken)
		);
	}

	private async Task RunLoopAsync(bool? mail, CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				if (mail != null)
					await ProcessBatchAsync(mail.Value, stoppingToken);
				else if (_nextMaintenance <= DateTime.UtcNow)
				{
					await MaintainAsync(stoppingToken);
					_nextMaintenance = DateTime.UtcNow.AddSeconds(30);
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception exception)
			{
				logger.LogWarning("Email worker infrastructure failed: {ExceptionType}", exception.GetType().Name);
			}

			try
			{
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
		}
	}

	private async Task ProcessBatchAsync(bool mail, CancellationToken ct)
	{
		foreach (var entry in await worker.ReadAsync(mail, _consumer, ct))
		{
			var attempt = await worker.TryLeaseAsync(mail, entry.StreamId, _consumer, ct);
			if (attempt == 0)
				continue;
			var item = entry with { Attempt = attempt };
			using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
			timeout.CancelAfter(TimeSpan.FromSeconds(30));
			await using var scope = scopes.CreateAsyncScope();
			var workflow = scope.ServiceProvider.GetRequiredService<EmailWorkflowService>();
			try
			{
				if (mail)
				{
					var message = item.Message!;
					var priorOutcome = await queue.FindMailOutcomeAsync(message.EventId, timeout.Token);
					if (priorOutcome != null)
					{
						await workflow.MarkMailAsync(
							message.UserId,
							message.EventId,
							priorOutcome == "acceptedByTransport",
							timeout.Token
						);
						await worker.CompleteAsync(true, item, _consumer, ct);
						continue;
					}
					if (attempt > options.Value.MaxAttempts)
					{
						await queue.SetMailOutcomeAsync(message, false, ct);
						await workflow.MarkMailAsync(message.UserId, message.EventId, false, timeout.Token);
						await worker.FailAsync(mail, item, _consumer, ct);
						continue;
					}
					if (await workflow.CanSendMailAsync(message, timeout.Token))
					{
						await transport.SendAsync(message, timeout.Token);
						await queue.SetMailOutcomeAsync(message, true, timeout.Token);
						await workflow.MarkMailAsync(message.UserId, message.EventId, true, timeout.Token);
					}
				}
				else
				{
					EmailOperationResult result;
					var terminal = await queue.FindTerminalResultAsync(item.Job!.Id, timeout.Token);
					if (terminal != null)
					{
						result = terminal;
						if (result.Status == "succeeded" && item.Job.TokenFingerprint != null)
							await queue.FinishTokenAsync(item.Job.TokenFingerprint, timeout.Token);
					}
					else if (attempt > options.Value.MaxAttempts)
						result =
							await workflow.FindProcessedAsync(item.Job!, timeout.Token)
							?? new EmailOperationResult(
								"failed",
								"processing_failed",
								"The operation could not be completed."
							);
					else
						result = await workflow.ProcessAsync(item.Job!, timeout.Token);
					await queue.SetResultAsync(item.Job!.Id, result, ct);
					if (result.Status is "succeeded" or "completed")
						await workflow.RecoverOperationAsync(item.Job, timeout.Token);
					if (attempt > options.Value.MaxAttempts)
					{
						await worker.FailAsync(false, item, _consumer, ct);
						continue;
					}
				}
				await worker.CompleteAsync(mail, item, _consumer, ct);
			}
			catch (Exception exception) when (!ct.IsCancellationRequested)
			{
				logger.LogWarning(
					"Email work item failed: Mail={Mail}, Attempt={Attempt}, Type={ExceptionType}",
					mail,
					attempt,
					exception.GetType().Name
				);
				if (attempt >= options.Value.MaxAttempts)
				{
					if (mail)
					{
						var outcome = await queue.FindMailOutcomeAsync(item.Message!.EventId, ct);
						if (outcome == null)
							await queue.SetMailOutcomeAsync(item.Message, false, ct);
						try
						{
							await workflow.MarkMailAsync(
								item.Message.UserId,
								item.Message.EventId,
								outcome == "acceptedByTransport",
								ct
							);
						}
						catch (Exception recoveryException) when (!ct.IsCancellationRequested)
						{
							logger.LogWarning(
								"Email transport outcome awaits database recovery: {ExceptionType}",
								recoveryException.GetType().Name
							);
						}
					}
					else
					{
						EmailOperationResult result;
						try
						{
							result =
								await workflow.FindProcessedAsync(item.Job!, ct)
								?? new EmailOperationResult(
									"failed",
									"processing_failed",
									"The operation could not be completed."
								);
						}
						catch (Exception recoveryException) when (!ct.IsCancellationRequested)
						{
							logger.LogWarning(
								"Email operation outcome awaits database recovery: {ExceptionType}",
								recoveryException.GetType().Name
							);
							result = new EmailOperationResult(
								"indeterminate",
								"processing_unavailable",
								"The database outcome is temporarily unavailable. Check the account state before repeating the operation."
							);
						}
						await queue.SetResultAsync(item.Job!.Id, result, ct);
					}
				}
				await worker.FailAsync(mail, item, _consumer, ct);
			}
		}
	}

	private async Task MaintainAsync(CancellationToken ct)
	{
		await using var scope = scopes.CreateAsyncScope();
		var store = scope.ServiceProvider.GetRequiredService<IEmailAccountStore>();
		var workflow = scope.ServiceProvider.GetRequiredService<EmailWorkflowService>();
		var userIds = await store.ListMaintenanceUserIdsAsync(_maintenanceCursor, options.Value.BatchSize, ct);
		foreach (var userId in userIds)
		{
			await workflow.RecoverAsync(userId, ct);
			await workflow.CleanupAsync(userId, ct);
			_maintenanceCursor = userId;
		}
		if (userIds.Count < options.Value.BatchSize)
			_maintenanceCursor = 0;
	}
}
