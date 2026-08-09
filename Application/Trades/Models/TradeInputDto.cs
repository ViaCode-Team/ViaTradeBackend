using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Trades.Models;

public record TradeInputDto
{
	public required DateTime OpenedAt { get; set; }

	public DateTime? ClosedAt { get; set; }

	public required double OpenPrice { get; set; }

	public double? ClosePrice { get; set; }

	public required TradeSignal Signal { get; set; }

	[Range(1, int.MaxValue)]
	public int Quantity { get; set; }

	public required int TradeTypeId { get; set; }

	public required int InstrumentId { get; set; }
}
