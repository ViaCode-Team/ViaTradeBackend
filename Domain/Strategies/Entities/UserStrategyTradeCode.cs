using Domain.Users.Entities;
using Domain.Strategies.Entities;
using Domain.Common;
using Domain.Entities.DataBase; // For User
using System.Text.Json.Serialization;

namespace Domain.Strategies.Entities;

public class UserStrategyTradeCode : AggregateRoot
{
	public int UserId { get; private set; }
	public int TradeCodeId { get; private set; }
	public int StrategyId { get; private set; }

	[JsonIgnore]
	public User? User { get; private set; }
	
	[JsonIgnore]
	public Domain.TradeCodes.Entities.TradeCode? TradeCode { get; private set; }
	
	[JsonIgnore]
	public TradeStrategy? TradeStrategy { get; private set; }

	private UserStrategyTradeCode() { }

	public UserStrategyTradeCode(int userId, int tradeCodeId, int strategyId)
	{
		UserId = userId;
		TradeCodeId = tradeCodeId;
		StrategyId = strategyId;
	}
}
