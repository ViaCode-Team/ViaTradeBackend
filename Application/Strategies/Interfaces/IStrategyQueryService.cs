using Application.Common.Models;
using Application.Notes.Models;
using Application.Strategies.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IStrategyQueryService
{
	Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<PageResult<TradeStrategy>> GetAsync(
		int userId,
		StrategyFilter filter,
		StrategySort sort,
		PageOptions page,
		CancellationToken ct
	);
	Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct);
	Task<PageResult<UserTradeStrategy>> GetUserLinkedAsync(int userId, PageOptions page, CancellationToken ct);
	Task<PageResult<UserStrategyTradeCode>> GetUserLinkedCodesAsync(int userId, PageOptions page, CancellationToken ct);
}
