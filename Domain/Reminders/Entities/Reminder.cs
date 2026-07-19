using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Reminders.Entities;

public sealed class Reminder : BaseEntity<int>
{
	[Column("TextRemind")]
	public string Text { get; set; }
	public DateTime DateTime { get; set; }
	public int TradeCodeId { get; set; }
	public int UserId { get; set; }
	public TradeCode? TradeCode { get; set; }
	public User? User { get; set; }
}
