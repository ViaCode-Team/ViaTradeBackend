using Domain.Entities;
using Domain.Trades.Entities;

namespace Domain.TradeCodes.Entities;

public sealed class TradeCode : BaseEntity<int>
{
	public required string ExchangeId { get; set; }

	public string? Description { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserTradeCode> UserTradeCodes { get; set; } = [];
}
