using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common.Abstractions;
using ViaTrade.Domain.Services;

namespace ViaTrade.Application.Trades.GetStatistics;

public sealed class GetTradeStatisticsHandler(ITradeRepository tradeStatistics)
	: IQueryHandler<GetTradeStatisticsQuery, TradeStatisticsResult>
{
	public async Task<TradeStatisticsResult> HandleAsync(GetTradeStatisticsQuery query, CancellationToken ct = default)
	{
		var result = await tradeStatistics.GetGlobalStatisticsAsync(query.UserId, ct);

		var tradeStatistic = new TradeCountsResult(result.TotalTrades, result.WinTrades, result.LoseTrades);

		var incomeStatistic = new TradeIncomeResult(
			Math.Round((decimal)result.TotalAbsoluteIncome, 2),
			TradeStatisticsCalcService.CalculateAverageIncome((decimal)result.TotalAbsoluteIncome, result.TotalTrades)
		);

		var winrateStatistic = new TradeWinrateResult(
			TradeStatisticsCalcService.CalculateWinrate(result.WinTrades, result.TotalTrades),
			TradeStatisticsCalcService.CalculateProfitFactor(result.TotalProfit, result.TotalLoss)
		);

		return new TradeStatisticsResult(tradeStatistic, incomeStatistic, winrateStatistic);
	}
}
