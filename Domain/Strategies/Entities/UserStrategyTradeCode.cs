using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Strategies.Entities;

public sealed class UserStrategyTradeCode : BaseEntity<int>
{
	public int UserId { get; set; }
	public int TradeCodeId { get; set; }
	public int StrategyId { get; set; }

	public User? User { get; set; }

	public TradeCode? TradeCode { get; set; }

	public TradeStrategy? TradeStrategy { get; set; }
}
