using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Models;

namespace ViaTrade.Application.Trades.Interfaces;

public interface ITradeQueryService
{
	Task<GlobalTradeStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<List<ProfitChartBucketDto>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct
	);
	Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct);
	Task<TradeDto> GetAsync(int userId, int id, CancellationToken ct);
	Task<PageResult<TradeDto>> GetPageAsync(
		int userId,
		TradeFilter tradeFilter,
		TradeSearch tradeSearch,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
