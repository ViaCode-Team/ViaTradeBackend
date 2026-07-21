using System.ComponentModel.DataAnnotations;
using Domain.Trades.Enums;

namespace Application.Trades.Models;

public record TradeInputDto
{
	public required DateTime DateOpen { get; set; }

	public DateTime? DateClose { get; set; }

	public required double TradeOpen { get; set; }

	public double? TradeClose { get; set; }

	public required TradeSignal TradeSignal { get; set; }

	[Range(1, int.MaxValue)]
	public int Count { get; set; }

	public required int TradeTypeId { get; set; }

	public required int TradeCodeId { get; set; }
}
