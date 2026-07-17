using Domain.Entities.CSV;

namespace Domain.Models.TradeLogic;

public class StrategyResultGroupe
{
	public Dictionary<string, List<StrategyResult>> Data { get; set; } = new();
}
