namespace Domain.Trades.Entities;

public class StrategyData
{
	public required string Name { get; set; }

	public List<TickerResults> Tickers { get; set; } = new();
}
