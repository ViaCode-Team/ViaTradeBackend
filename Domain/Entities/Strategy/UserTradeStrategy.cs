using Domain.Entities;
using Domain.Users.Entities;

namespace Domain.Strategies.Entities;

public sealed class UserTradeStrategy : BaseEntity<int>
{
	public required int UserId { get; set; }
	public required int TradeStrategyId { get; set; }

	public User? User { get; set; }
	public TradeStrategy? TradeStrategy { get; set; }
}
