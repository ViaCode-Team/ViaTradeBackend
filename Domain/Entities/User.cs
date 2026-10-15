using ViaTrade.Domain.Entities.Abstractions;

namespace ViaTrade.Domain.Entities;

public sealed class User : BaseEntity<int>
{
	public required string Login { get; set; }

	public required string PasswordHash { get; set; }

	public string? NormalizedLogin { get; set; }

	public string? Email { get; set; }

	public string? NormalizedEmail { get; set; }

	public DateTime? EmailConfirmedAt { get; set; }

	public long AuthorizationVersion { get; set; }

	public Guid? EmailConfirmationOperationId { get; set; }

	public DateTime? EmailConfirmationExpiresAt { get; set; }

	public Guid? PasswordResetOperationId { get; set; }

	public DateTime? PasswordResetExpiresAt { get; set; }

	public string? PendingEmail { get; set; }

	public string? NormalizedPendingEmail { get; set; }

	public Guid? EmailChangeOperationId { get; set; }

	public DateTime? EmailChangeExpiresAt { get; set; }

	public string? PreviousEmail { get; set; }

	public string? NormalizedPreviousEmail { get; set; }

	public Guid? CompletedEmailChangeId { get; set; }

	public DateTime? EmailRecoveryExpiresAt { get; set; }

	public string? EmailWorkStateJson { get; set; }

	public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

	public required DateTime RegisteredAt { get; set; }

	public string? TelegramId { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserStrategy> UserStrategies { get; set; } = [];
}
