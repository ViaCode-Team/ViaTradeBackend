namespace ViaTrade.Api.Contracts.Statistics;

public record GlobalStatisticResponse(
	TradeStatisticResponse TradeStatistic,
	IncomeTradeStatisticResponse IncomeStatistic,
	WinrateTradeStatisticResponse WinrateStatistic
);
