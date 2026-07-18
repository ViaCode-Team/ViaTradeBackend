using Domain.Common;
using Domain.Users.Entities;
namespace Domain.TradeCodes.Entities;

public sealed class UserTradeCode : DomainEntity<int>
{
	public int UserId { get; set; }

	public int TradeCodeId { get; set; }

	public required User User { get; set; }

	public required TradeCode TradeCode { get; set; }
}
