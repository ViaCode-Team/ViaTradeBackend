using Domain.Trades.Entities;

namespace Application.Trades.Models;

public record StrategyResults(List<StrategyData> Strategies)
{
	public StrategyResults()
		: this([]) { }
}
