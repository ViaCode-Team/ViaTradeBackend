namespace ViaTrade.Application.Email;

public enum EmailAction
{
	Register,
	ConfirmRegistration,
	ResendConfirmation,
	RequestPasswordReset,
	ResetPassword,
	StartEmailChange,
	ConfirmEmailChange,
	RecoverEmail,
}

public enum EmailTokenPurpose
{
	Registration,
	PasswordReset,
	EmailChange,
	EmailRecovery,
}

public sealed record EmailJob
{
	public required Guid Id { get; init; }
	public required EmailAction Action { get; init; }
	public required DateTime CreatedAt { get; init; }
	public required DateTime ExpiresAt { get; init; }
	public string? Login { get; init; }
	public string? Email { get; init; }
	public string? PasswordHash { get; init; }
	public string? TokenFingerprint { get; init; }
	public int? UserId { get; init; }
	public string? SessionId { get; init; }
	public long? VerifiedAuthorizationVersion { get; init; }

	public bool IsPrivateRequest => Action is EmailAction.ResendConfirmation or EmailAction.RequestPasswordReset;
}

public sealed record EmailToken(
	EmailTokenPurpose Purpose,
	int UserId,
	Guid OperationId,
	long AuthorizationVersion,
	DateTime ExpiresAt
);

public sealed record EmailAcceptedResult(Guid OperationId, string ResultAccessKey, DateTime WaitUntil);

public sealed record EmailOperationResult(string Status, string Code, string Message, bool ClearCookies = false);

public sealed record EmailReceipt(Guid Id, DateTime RetainUntil, EmailOperationResult Result);

public sealed record EmailIntent
{
	public required Guid Id { get; init; }
	public required string Kind { get; init; }
	public required string Recipient { get; init; }
	public required DateTime ExpiresAt { get; init; }
	public EmailToken? Token { get; init; }
	public DateTime? AcceptedByTransportAt { get; set; }
	public bool PermanentlyFailed { get; set; }
}

public sealed class EmailWorkState
{
	public List<EmailReceipt> Receipts { get; set; } = [];
	public List<EmailIntent> Intents { get; set; } = [];
	public long? RevokeSessionsBeforeVersion { get; set; }
	public int SessionRevocationOffset { get; set; }
}

public sealed record EmailMessage(
	Guid EventId,
	int UserId,
	string Recipient,
	string Kind,
	string? Token,
	DateTime ExpiresAt
);

public sealed record EmailWorkItem(string StreamId, EmailJob? Job, EmailMessage? Message, int Attempt);
