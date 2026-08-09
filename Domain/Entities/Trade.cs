using Domain.Enums;

namespace Domain.Entities;

public sealed class Trade : BaseEntity<int>
{
	public required DateTime OpenedAt { get; set; }
	public DateTime? ClosedAt { get; set; }
	public required double OpenPrice { get; set; }
	public double? ClosePrice { get; set; }
	public required int Quantity { get; set; }

	public required decimal TotalPrice { get; set; }

	public required int TradeTypeId { get; set; }
	public required int InstrumentId { get; set; }
	public required int UserId { get; set; }
	public required TradeSignal Signal { get; set; }

	public TradeType? TradeType { get; set; }

	public Instrument? Instrument { get; set; }

	public User? User { get; set; }
}
