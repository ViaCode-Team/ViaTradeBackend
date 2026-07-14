using Domain.Entities.CSV;

namespace Domain.Models.TradeLogic;

public class TickerResults
{
	public required string TradeCode { get; set; }
	public int? Accuracy { get; set; }

	public List<StrategyResult> Results { get; set; } = new();
}
