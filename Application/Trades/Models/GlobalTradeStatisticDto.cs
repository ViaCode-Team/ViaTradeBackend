namespace ViaTrade.Application.Trades.Models;

public record GlobalTradeStatisticDto(
	TradeStatisticDto TradeStatistic,
	IncomeTradeStatisticDto IncomeStatistic,
	WinrateTradeStatisticDto WinrateStatistic
);
