using Application.Common.Queries;
using Application.Statistics.Models;
using Application.Strategies.Queries;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IStrategyQueryService
{
	Task<StrategyStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<PageResult<TradeStrategy>> GetAsync(int userId, StrategyFilter filter, StrategySort sort, PageOptions page, CancellationToken ct);
	Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct);
	Task<PageResult<UserTradeStrategy>> GetUserLinkedAsync(int userId, PageOptions page, CancellationToken ct);
	Task<PageResult<UserStrategyTradeCode>> GetUserLinkedCodesAsync(int userId, PageOptions page, CancellationToken ct);
}
