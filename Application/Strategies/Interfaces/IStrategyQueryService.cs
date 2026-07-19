using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Statistics.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IStrategyQueryService
{
	Task<StrategyStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<PagedResult<TradeStrategy>> GetAsync(int userId, StrategyFilterRequest filterRequest, StrategySortRequest sortRequest, PaginationRequest paginationRequest, CancellationToken ct);
	Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct);
	Task<PagedResult<UserTradeStrategy>> GetUserLinkedAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct);
	Task<PagedResult<UserStrategyTradeCode>> GetUserLinkedCodesAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct);
}
