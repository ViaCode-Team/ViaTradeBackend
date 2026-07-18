using Domain.Users.Entities;
using Domain.Common;
using Domain.Strategies.Entities;
using Domain.Trades.Entities;
using System.ComponentModel.DataAnnotations;

namespace Domain.Users.Entities;

public class User : AggregateRoot
{
	[MaxLength(64)]
	public string Login { get; private set; }

	[MaxLength(512)]
	public string HashPassword { get; private set; }

	public DateTime LastLoginDate { get; private set; } = DateTime.UtcNow;

	public DateTime RegisterDate { get; private set; }

	[MaxLength(512)]
	public string? TgId { get; private set; }

	public ICollection<Trade>? Trades { get; private set; }
	public ICollection<UserTradeStrategy>? UserTradeStrategies { get; private set; }

	private User() { }

	public User(string login, string hashPassword, DateTime registerDate)
	{
		Login = login;
		HashPassword = hashPassword;
		RegisterDate = registerDate;
	}

	public void UpdateLastLoginDate()
	{
		LastLoginDate = DateTime.UtcNow;
	}

	public void LinkTelegram(string tgId)
	{
		TgId = tgId;
	}
}
