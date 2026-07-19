using Domain.Common;
using Domain.Strategies.Entities;
using Domain.Trades.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Users.Entities;

public sealed class User : BaseEntity<int>
{
	public string Login { get; set; }

	public string HashPassword { get; set; }

	public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

	public DateTime RegisterDate { get; set; }

	[Column("TgId")]
	public string? TelegramId { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserTradeStrategy> UserTradeStrategies { get; set; } = [];

}
