using Domain.Entities.DataBase;
using System.Linq.Expressions;

namespace Domain.Services;

public class TradeGroupAggregation
{
	public int TotalTrades { get; set; }
	public int WinTrades { get; set; }
	public int LoseTrades { get; set; }
	public double TotalAbsoluteIncome { get; set; }
	public double TotalProfit { get; set; }
	public double TotalLoss { get; set; }
}

public static class TradeStatisticsCalcService
{
	public static IQueryable<TradeGroupAggregation> CalculateAggregate(IQueryable<Trade> query)
	{
		return query
			.Where(t => t.NetIncome.HasValue)
			.Select(t => new
			{
				IsWin = t.NetIncome > 0,
				IsLose = t.NetIncome < 0,
				AbsoluteIncome = ((t.TradeClose ?? 0) - t.TradeOpen) * t.Count * (int)t.TradeSignal
			})
			.Select(t => new
			{
				t.IsWin,
				t.IsLose,
				t.AbsoluteIncome,
				Profit = t.IsWin ? Math.Abs(t.AbsoluteIncome) : 0,
				Loss = t.IsLose ? Math.Abs(t.AbsoluteIncome) : 0
			})
			.GroupBy(t => 1)
			.Select(g => new TradeGroupAggregation
			{
				TotalTrades = g.Count(),
				WinTrades = g.Count(t => t.IsWin),
				LoseTrades = g.Count(t => t.IsLose),
				TotalAbsoluteIncome = g.Sum(t => t.AbsoluteIncome),
				TotalProfit = g.Sum(t => t.Profit),
				TotalLoss = g.Sum(t => t.Loss)
			});
	}

	// Expression for LINQ (translation to SQL)
	public static Expression<Func<Trade, double>> AbsoluteIncomeExpression =>
		trade => ((trade.TradeClose ?? 0) - trade.TradeOpen) * trade.Count * (int)trade.TradeSignal;

	public static Expression<Func<Trade, double>> AbsoluteIncomeAbsExpression =>
		trade => Math.Abs(((trade.TradeClose ?? 0) - trade.TradeOpen) * trade.Count * (int)trade.TradeSignal);

	public static double CalculateAbsoluteIncome(Trade trade)
	{
		if (trade.TradeClose == null)
			return 0;

		return (trade.TradeClose.Value - trade.TradeOpen) * trade.Count * (int)trade.TradeSignal;
	}

	public static float CalculateProfitFactor(double totalProfit, double totalLoss)
	{
		if (totalLoss > 0)
			return (float)Math.Round(totalProfit / totalLoss, 3);

		if (totalProfit > 0)
			return float.PositiveInfinity;

		return 0f;
	}

	public static float CalculateWinrate(int winTrades, int totalTrades)
	{
		if (totalTrades == 0)
			return 0f;

		return (float)Math.Round((double)winTrades / totalTrades * 100, 2);
	}

	public static decimal CalculateAverageIncome(decimal totalIncome, int totalTrades)
	{
		if (totalTrades == 0)
			return 0m;

		return Math.Round(totalIncome / totalTrades, 2);
	}
}
