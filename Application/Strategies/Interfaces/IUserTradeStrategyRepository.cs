using Application.Common.Interfaces.Repositories;
using Application.Common.Queries;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserTradeStrategyRepository : IRepository<UserTradeStrategy>
{
	Task<PageResult<UserTradeStrategy>> GetByUserPagedAsync(int userId, PageOptions page, CancellationToken ct);
	Task<int> CountByUserAsync(int userId, CancellationToken ct);
	Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(int userId, CancellationToken ct);
}
