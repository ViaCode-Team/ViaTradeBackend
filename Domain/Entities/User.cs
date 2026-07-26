namespace Domain.Entities;

public sealed class User : BaseEntity<int>
{
	public required string Login { get; set; }

	public required string PasswordHash { get; set; }

	public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

	public required DateTime RegisteredAt { get; set; }

	public string? TelegramId { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserStrategy> UserStrategies { get; set; } = [];
}
