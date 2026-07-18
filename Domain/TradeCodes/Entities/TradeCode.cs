using Domain.Common;
using Domain.Trades.Entities;

namespace Domain.TradeCodes.Entities;

public sealed class TradeCode : AggregateRoot<int>
{
	public string ExchangeId { get; private set; }

	public string? Description { get; private set; }

	private readonly List<Trade> _trades = [];
	public IReadOnlyCollection<Trade> Trades => _trades.AsReadOnly();

	private readonly List<UserTradeCode> _userTradeCodes = [];
	public IReadOnlyCollection<UserTradeCode> UserTradeCodes => _userTradeCodes.AsReadOnly();

	private TradeCode() { }

	public TradeCode(string exchangeId, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(exchangeId))
			throw new ArgumentException("ExchangeId cannot be empty.", nameof(exchangeId));

		ExchangeId = exchangeId;
		Description = description;
	}
}
