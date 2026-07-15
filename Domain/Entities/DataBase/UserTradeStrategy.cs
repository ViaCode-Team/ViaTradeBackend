using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase;

public class UserTradeStrategy : BaseEntity
{
	public required int UserId { get; set; }

	public required int TradeStrategyId { get; set; }
	[JsonIgnore]
	public User? User { get; set; }
	[JsonIgnore]
	public TradeStrategy? TradeStrategy { get; set; }
}
