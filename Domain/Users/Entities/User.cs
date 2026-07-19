using Domain.Common;
using Domain.Strategies.Entities;
using Domain.Trades.Entities;

namespace Domain.Users.Entities;

public sealed class User : AggregateRoot<int>
{
	public string Login { get; private set; }

	public string HashPassword { get; private set; }

	public DateTime LastLoginDate { get; private set; } = DateTime.UtcNow;

	public DateTime RegisterDate { get; private set; }

	public string? TgId { get; private set; }

	private readonly List<Trade> _trades = [];
	public IReadOnlyCollection<Trade> Trades => _trades.AsReadOnly();

	private readonly List<UserTradeStrategy> _userTradeStrategies = [];
	public IReadOnlyCollection<UserTradeStrategy> UserTradeStrategies => _userTradeStrategies.AsReadOnly();

	private User() { }

	public User(string login, string hashPassword, DateTime registerDate)
	{
		if (string.IsNullOrWhiteSpace(login))
			throw new ArgumentException("Login cannot be empty.", nameof(login));
		if (string.IsNullOrWhiteSpace(hashPassword))
			throw new ArgumentException("Password hash cannot be empty.", nameof(hashPassword));

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
