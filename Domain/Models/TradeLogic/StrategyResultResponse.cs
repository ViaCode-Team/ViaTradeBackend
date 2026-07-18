namespace Domain.Models.TradeLogic;

public record StrategyResultResponse(
	List<StrategyData> Strategies
)
{
	public StrategyResultResponse() : this([]) { }
}
