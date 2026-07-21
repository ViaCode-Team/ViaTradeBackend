using Domain.Models.Trade;

namespace Application.Trades.Models;

public record StrategyResults(List<StrategyData> Strategies)
{
	public StrategyResults()
		: this([]) { }
}
