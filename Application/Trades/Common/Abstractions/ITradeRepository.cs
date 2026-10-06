using ViaTrade.Application.Trades.GetDateRange;
using ViaTrade.Application.Trades.GetProfitChart;
using ViaTrade.Application.Trades.GetStatistics;
using ViaTrade.Domain.Enums;

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
	Task<int> ExecuteUpdateAsync(
		int userId,
		int id,
		int instrumentId,
		int tradeTypeId,
		DateTime openedAt,
		DateTime? closedAt,
		double openPrice,
		double? closePrice,
		TradeSignal signal,
		int quantity,
		decimal price,
		CancellationToken ct = default
	);
}
