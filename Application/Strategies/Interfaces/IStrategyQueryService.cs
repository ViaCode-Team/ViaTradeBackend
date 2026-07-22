using Application.Common.Models;
using Application.Notes.Models;
using Application.Strategies.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IStrategyQueryService
{
	Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<PageResult<TradeStrategy>> GetPageAsync(
		int userId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct);
	Task<PageResult<UserTradeStrategy>> GetUserStrategiesPageAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<PageResult<UserStrategyTradeCode>> GetUserStrategyTradeCodesPageAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
