namespace ViaTrade.Application.Email;

public interface IEmailQueue
{
	Task<EmailAcceptedResult> PublishAsync(EmailJob job, CancellationToken ct);

	Task<EmailOperationResult?> FindResultAsync(
		int? userId,
		Guid operationId,
		string? sessionId,
		string accessKey,
		CancellationToken ct
	);

	Task<EmailOperationResult?> FindTerminalResultAsync(Guid operationId, CancellationToken ct);

	Task<EmailToken?> FindTokenAsync(string fingerprint, CancellationToken ct);

	Task FinishTokenAsync(string fingerprint, CancellationToken ct);

	Task PublishMailAsync(int userId, EmailIntent intent, CancellationToken ct);

	Task<string?> FindMailOutcomeAsync(Guid eventId, CancellationToken ct);

	Task SetMailOutcomeAsync(EmailMessage message, bool accepted, CancellationToken ct);

	Task SetResultAsync(Guid operationId, EmailOperationResult result, CancellationToken ct);

	Task<bool> TryAcquireLimitAsync(string scope, string value, int limit, TimeSpan window, CancellationToken ct);

	Task<bool> TryAcquireMailCooldownAsync(string address, EmailAction action, Guid jobId, CancellationToken ct);
}

public interface IEmailStreamWorker
{
	Task InitializeAsync(CancellationToken ct);

	Task<IReadOnlyList<EmailWorkItem>> ReadAsync(bool mail, string consumer, CancellationToken ct);

	Task<int> TryLeaseAsync(bool mail, string streamId, string consumer, CancellationToken ct);

	Task CompleteAsync(bool mail, EmailWorkItem item, string consumer, CancellationToken ct);

	Task FailAsync(bool mail, EmailWorkItem item, string consumer, CancellationToken ct);
}

public interface IEmailTransport
{
	Task SendAsync(EmailMessage message, CancellationToken ct);
}
