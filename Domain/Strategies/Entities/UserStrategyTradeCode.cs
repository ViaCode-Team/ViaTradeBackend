using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Strategies.Entities;

public sealed class UserStrategyTradeCode : AggregateRoot<int>
{
	public int UserId { get; private set; }
	public int TradeCodeId { get; private set; }
	public int StrategyId { get; private set; }

	public User? User { get; private set; }

	public TradeCode? TradeCode { get; private set; }

	public TradeStrategy? TradeStrategy { get; private set; }

	private UserStrategyTradeCode() { }

	public UserStrategyTradeCode(int userId, int tradeCodeId, int strategyId)
	{
		UserId = userId;
		TradeCodeId = tradeCodeId;
		StrategyId = strategyId;
	}
}
