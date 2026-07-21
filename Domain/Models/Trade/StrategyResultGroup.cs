namespace Domain.Models.Trade;

public class StrategyResultGroup
{
	public Dictionary<string, List<StrategyResult>> Data { get; set; } = [];
}
