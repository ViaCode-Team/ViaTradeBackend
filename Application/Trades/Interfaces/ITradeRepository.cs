using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Trades.Models;
using Domain.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeRepository : IRepository<Trade>
{
	Task<TradeStatisticAggregateDto> GetGlobalStatisticsAsync(int userId, CancellationToken ct = default);
	Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter filter,
		CancellationToken ct = default
	);
	Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct = default);
	Task<TradeProjectionDto?> FindProjectionByUserAndIdAsync(int userId, int id, CancellationToken ct = default);
	Task<PageResult<TradeProjectionDto>> GetPageProjectionAsync(
		IQuerySpecification<Trade> specification,
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
