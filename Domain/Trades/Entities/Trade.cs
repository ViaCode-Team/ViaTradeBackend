using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Trades.Enums;
using Domain.Users.Entities;

namespace Domain.Trades.Entities;

public sealed class Trade : BaseEntity<int>
{
	public DateTime DateOpen { get; set; }
	public DateTime? DateClose { get; set; }
	public double TradeOpen { get; set; }
	public double? TradeClose { get; set; }
	public double? NetIncome { get; set; }
	public int Count { get; set; }

	public decimal Price { get; set; }

	public int TradeTypeId { get; set; }
	public int TradeCodeId { get; set; }
	public int UserId { get; set; }
	public TradeSignal TradeSignal { get; set; }

	public TradeType? TradeType { get; set; }

	public TradeCode? TradeCode { get; set; }

	public User? User { get; set; }
}
