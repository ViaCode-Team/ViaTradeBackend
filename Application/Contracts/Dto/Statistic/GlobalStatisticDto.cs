namespace Application.Contracts.Dto.Statistic;

public class GlobalStatisticDto
{
	public required TradeStatisticDto TradeStatisticDto { get; set; }

	public required IncomeTradeStatisticDto IncomeStatistic { get; set; }

	public required WinrateTradeStatisticDto WinrateStatistic { get; set; }
}
