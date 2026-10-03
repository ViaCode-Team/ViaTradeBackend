using ViaTrade.Application.Trades.Models;

namespace ViaTrade.Application.Trades.Interfaces;

public interface ITradeRepository
{
	Task<TradeStatisticAggregateDto> GetGlobalStatisticsAsync(int userId, CancellationToken ct = default);
	Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct = default
	);
	Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateAsync(
		int userId,
		int id,
		TradeInputDto request,
		decimal price,
		CancellationToken ct = default
	);
}
