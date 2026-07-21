namespace ViaTradeBackend.Contracts.Statistics;

public record GlobalStatisticResponse(
	TradeStatisticResponse TradeStatistic,
	IncomeTradeStatisticResponse IncomeStatistic,
	WinrateTradeStatisticResponse WinrateStatistic
);
