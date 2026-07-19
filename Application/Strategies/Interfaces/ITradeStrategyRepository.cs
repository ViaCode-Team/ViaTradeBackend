using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Queries;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface ITradeStrategyRepository : IRepository<TradeStrategy>
{
	Task<int> CountAsync(CancellationToken ct = default);
	Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken ct = default);
	Task<PageResult<TradeStrategy>> GetPagedFilteredAsync(int userId, IQuerySpecification<TradeStrategy> spec, PageOptions page, CancellationToken ct = default);
}
