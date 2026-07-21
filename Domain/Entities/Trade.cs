using Domain.TradeCodes.Entities;
using Domain.Trades.Enums;
using Domain.Users.Entities;

namespace Domain.Entities;

public sealed class Trade : BaseEntity<int>
{
	public required DateTime DateOpen { get; set; }
	public DateTime? DateClose { get; set; }
	public required double TradeOpen { get; set; }
	public double? TradeClose { get; set; }
	public required int Count { get; set; }

	public required decimal Price { get; set; }

	public required int TradeTypeId { get; set; }
	public required int TradeCodeId { get; set; }
	public required int UserId { get; set; }
	public required TradeSignal TradeSignal { get; set; }

	public TradeType? TradeType { get; set; }

	public TradeCode? TradeCode { get; set; }

	public User? User { get; set; }
}
