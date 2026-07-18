using Domain.Entities.CSV;

namespace Domain.Models.TradeLogic;

public class StrategyResultGroup
{
	public Dictionary<string, List<StrategyResult>> Data { get; set; } = [];
}
