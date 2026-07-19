using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Application.Statistics.Models;
using Application.Trades.Models;
using Domain.Trades.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeRepository : IRepository<Trade>
{
	Task<GlobalStatisticReadModel> GetGlobalStatisticAsync(int userId, CancellationToken ct);
	Task<PagedResult<Trade>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct);
	Task<PagedResult<Trade>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken ct);
	Task<PagedResult<Trade>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PaginationRequest paginationRequest, CancellationToken ct);
	Task<int> UpdateAsync(int id, int userId, TradeCreateDto request, double? netIncome, decimal price, CancellationToken ct = default);
}
