using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Interfaces;

public interface ITradeRepository : IRepository<Trade>
{
	Task<TradeStatisticAggregateDto> GetGlobalStatisticsAsync(int userId, CancellationToken ct = default);
	Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct = default
	);
	Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct = default);
	Task<TradeProjectionDto?> FindProjectionByUserAndIdAsync(int userId, int id, CancellationToken ct = default);
	Task<PageResult<TradeProjectionDto>> GetPageProjectionAsync(
		IQueryObject<Trade> queryObject,
		PageOptions pageOptions,
		CancellationToken ct = default
	);

	Task<int> ExecuteUpdateAsync(
		int userId,
		int id,
		TradeInputDto request,
		decimal price,
		CancellationToken ct = default
	);
}
