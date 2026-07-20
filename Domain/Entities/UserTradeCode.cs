using Domain.Entities;
using Domain.Users.Entities;

namespace Domain.TradeCodes.Entities;

public sealed class UserTradeCode : BaseEntity<int>
{
	public required int UserId { get; set; }

	public required int TradeCodeId { get; set; }

	public required User User { get; set; }

	public required TradeCode TradeCode { get; set; }
}
