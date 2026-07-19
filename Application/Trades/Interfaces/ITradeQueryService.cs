using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Statistics.Models;
using Domain.Trades.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeQueryService
{
	Task<GlobalStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Trade> GetAsync(int id, int userId, CancellationToken ct);
	Task<PagedResult<Trade>> GetAsync(int userId, TradeFilterRequest? filterRequest, PaginationRequest? paginationRequest, CancellationToken ct);
}
