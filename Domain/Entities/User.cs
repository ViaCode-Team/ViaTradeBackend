using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;
using Domain.Strategies.Entities;

namespace Domain.Users.Entities;

public sealed class User : BaseEntity<int>
{
	public required string Login { get; set; }

	public required string HashPassword { get; set; }

	public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

	public required DateTime RegisterDate { get; set; }

	[Column("TgId")]
	public string? TelegramId { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserTradeStrategy> UserTradeStrategies { get; set; } = [];
}
