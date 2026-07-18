using Domain.Strategies.Entities;
using Domain.Common;
using Domain.Entities.DataBase; // For User (temporary)

namespace Domain.Strategies.Entities;

public class UserTradeStrategy : AggregateRoot
{
	public int UserId { get; private set; }
	public int TradeStrategyId { get; private set; }

	public User? User { get; private set; }
	public TradeStrategy? TradeStrategy { get; private set; }

	private UserTradeStrategy() { }

	public UserTradeStrategy(int userId, int tradeStrategyId)
	{
		UserId = userId;
		TradeStrategyId = tradeStrategyId;
	}
}
