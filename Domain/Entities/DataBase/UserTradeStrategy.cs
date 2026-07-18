namespace Domain.Entities.DataBase;

public class UserTradeStrategy : BaseEntity
{
	public required int UserId { get; set; }

	public required int TradeStrategyId { get; set; }

	public User? User { get; set; }

	public TradeStrategy? TradeStrategy { get; set; }
}
