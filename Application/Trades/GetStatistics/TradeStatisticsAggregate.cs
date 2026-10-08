namespace ViaTrade.Application.Trades.GetStatistics;

public record TradeStatisticsAggregate(
	int TotalTrades,
	int WinTrades,
	int LoseTrades,
	double TotalAbsoluteIncome,
	double TotalProfit,
	double TotalLoss
)
{
	public static TradeStatisticsAggregate Empty { get; } =
		new(TotalTrades: 0, WinTrades: 0, LoseTrades: 0, TotalAbsoluteIncome: 0, TotalProfit: 0, TotalLoss: 0);
}
