namespace Domain.Trades.Entities;

public record StrategyResultResponse(
	List<StrategyData> Strategies
)
{
	public StrategyResultResponse() : this([]) { }
}
