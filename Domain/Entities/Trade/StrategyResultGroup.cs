namespace Domain.Trades.Entities;

public class StrategyResultGroup
{
	public Dictionary<string, List<StrategyResult>> Data { get; set; } = [];
}
