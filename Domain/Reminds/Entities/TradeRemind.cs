using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Reminds.Entities;

public sealed class TradeRemind : BaseEntity<int>
{
	public string TextRemind { get; set; }
	public DateTime DateTime { get; set; }
	public int TradeCodeId { get; set; }
	public int UserId { get; set; }
	public TradeCode? TradeCode { get; set; }
	public User? User { get; set; }

}
