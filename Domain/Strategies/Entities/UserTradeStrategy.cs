using Domain.Common;
using Domain.Users.Entities;

namespace Domain.Strategies.Entities;

public sealed class UserTradeStrategy : BaseEntity<int>
{
	public int UserId { get; set; }
	public int TradeStrategyId { get; set; }

	public User? User { get; set; }
	public TradeStrategy? TradeStrategy { get; set; }

}
