using ViaTrade.Application.Trades.GetDateRange;
using ViaTrade.Application.Trades.GetProfitChart;
using ViaTrade.Application.Trades.GetStatistics;

namespace ViaTrade.Application.Trades.Common.Abstractions;

public interface ITradeRepository
{
	Task<TradeStatisticsAggregate> GetGlobalStatisticsAsync(int userId, CancellationToken ct = default);
	Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct = default
	);
	Task<TradeDateRangeResult> GetTradeDateRangeAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateAsync(int userId, int id, TradeInput request, decimal price, CancellationToken ct = default);
}
