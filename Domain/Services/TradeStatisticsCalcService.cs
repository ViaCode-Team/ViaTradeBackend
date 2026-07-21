using System.Linq.Expressions;
using Domain.Entities;
using Domain.Trades.Enums;

namespace Domain.Statistics.Services;

public static class TradeStatisticsCalcService
{
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

	public static double? CalculateNetIncome(double tradeOpen, double? tradeClose, TradeSignal tradeSignal)
	{
		if (tradeClose == null || tradeOpen == 0 || tradeSignal == TradeSignal.HOLD)
			return null;

		var basePercent = (tradeClose.Value - tradeOpen) / tradeOpen * 100;
		double adjustedPercent = basePercent;
		if (tradeSignal == TradeSignal.SELL)
		{
			adjustedPercent = -basePercent;
		}

		return Math.Round(adjustedPercent, 2);
	}
}
