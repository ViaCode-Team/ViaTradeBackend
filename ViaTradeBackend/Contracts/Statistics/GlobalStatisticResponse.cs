namespace ViaTradeBackend.Contracts.Statistics;

public record GlobalStatisticResponse(
	TradeStatisticResponse TradeStatisticResponse,
	IncomeTradeStatisticResponse IncomeStatistic,
	WinrateTradeStatisticResponse WinrateStatistic
);

