namespace Application.Statistics.Models;

public class GlobalStatisticReadModel
{
	public required TradeStatisticReadModel TradeStatisticReadModel { get; set; }

	public required IncomeTradeStatisticReadModel IncomeStatistic { get; set; }

	public required WinrateTradeStatisticReadModel WinrateStatistic { get; set; }
}

