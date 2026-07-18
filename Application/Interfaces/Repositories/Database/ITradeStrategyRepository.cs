using Domain.Strategies.Entities;
using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeStrategyRepository : IRepository<TradeStrategy>
{
	Task<int> CountAsync(CancellationToken cancellationToken = default);
	Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeStrategy>> GetPagedFilteredAsync(int userId, IQuerySpecification<TradeStrategy> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken = default);
}
