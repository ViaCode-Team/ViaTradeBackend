namespace Application.Trades.Models;

public record TradeStatisticAggregateDto(
	int TotalTrades,
	int WinTrades,
	int LoseTrades,
	double TotalAbsoluteIncome,
	double TotalProfit,
	double TotalLoss
)
{
	public static TradeStatisticAggregateDto Empty { get; } =
		new(TotalTrades: 0, WinTrades: 0, LoseTrades: 0, TotalAbsoluteIncome: 0, TotalProfit: 0, TotalLoss: 0);
}
