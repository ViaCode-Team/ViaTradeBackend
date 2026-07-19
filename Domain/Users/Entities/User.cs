using Domain.Common;
using Domain.Strategies.Entities;
using Domain.Trades.Entities;

namespace Domain.Users.Entities;

public sealed class User : BaseEntity<int>
{
	public string Login { get; set; }

	public string HashPassword { get; set; }

	public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

	public DateTime RegisterDate { get; set; }

	public string? TgId { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserTradeStrategy> UserTradeStrategies { get; set; } = [];

}
