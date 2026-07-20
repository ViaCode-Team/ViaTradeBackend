using Domain.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Strategies.Entities;

public sealed class UserStrategyTradeCode : BaseEntity<int>
{
	public required int UserId { get; set; }
	public required int TradeCodeId { get; set; }
	public required int StrategyId { get; set; }

	public User? User { get; set; }

	public TradeCode? TradeCode { get; set; }

	public TradeStrategy? TradeStrategy { get; set; }
}
