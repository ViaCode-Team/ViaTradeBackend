namespace ViaTrade.Application.Trades.GetStatistics;

public record TradeStatisticsResult(
	TradeCountsResult TradeStatistic,
	TradeIncomeResult IncomeStatistic,
	TradeWinrateResult WinrateStatistic
);
