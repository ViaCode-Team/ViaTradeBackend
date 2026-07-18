using Domain.Entities.DataBase;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Dto.Requests.Trade;

public record TradeCreateDto
{
	public required DateTime DateOpen { get; set; }

	public DateTime? DateClose { get; set; }

	public required double TradeOpen { get; set; }

	public double? TradeClose { get; set; }

	public required TradeSignal TradeSignal { get; set; }

	[Range(0, int.MaxValue)]
	public int Count { get; set; }

	public required int TradeTypeId { get; set; }

	public required int TradeCodeId { get; set; }
}
