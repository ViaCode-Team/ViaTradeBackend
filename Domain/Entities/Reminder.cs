using Domain.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Reminders.Entities;

public sealed class Reminder : BaseEntity<int>
{
	public required string TextRemind { get; set; }
	public required DateTime DateTime { get; set; }
	public required int TradeCodeId { get; set; }
	public required int UserId { get; set; }
	public TradeCode? TradeCode { get; set; }
	public User? User { get; set; }
}
