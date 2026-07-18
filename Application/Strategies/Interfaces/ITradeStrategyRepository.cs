using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface ITradeStrategyRepository : IRepository<TradeStrategy>
{
	Task<int> CountAsync(CancellationToken cancellationToken = default);
	Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeStrategy>> GetPagedFilteredAsync(int userId, IQuerySpecification<TradeStrategy> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken = default);
}
