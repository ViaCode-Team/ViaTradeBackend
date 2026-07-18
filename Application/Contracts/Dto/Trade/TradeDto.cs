using Domain.Entities.DataBase;

namespace Application.Contracts.Dto.Trade;

public class TradeDto
{
	public required int Id { get; set; }

	public DateTime DateOpen { get; set; }

	public DateTime? DateClose { get; set; }

	public double TradeOpen { get; set; }

	public double? TradeClose { get; set; }

	public double? NetIncome { get; set; }

	public int Count { get; set; }

	public decimal Price { get; set; }

	public required TradeSignal TradeSignal { get; set; }

	public int TradeTypeId { get; set; }

	public int TradeCodeId { get; set; }

	public int UserId { get; set; }
}
