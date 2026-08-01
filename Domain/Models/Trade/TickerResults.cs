namespace Domain.Models.Trade;

public class TickerResults
{
	public required string Instrument { get; set; }

	public int? Accuracy { get; set; }

	public List<StrategyResult> Results { get; set; } = [];
}
