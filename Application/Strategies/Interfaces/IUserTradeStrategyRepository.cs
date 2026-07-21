using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserTradeStrategyRepository : IRepository<UserTradeStrategy>
{
	Task<int> CountByUserAsync(int userId, CancellationToken ct);
	Task<PageResult<UserTradeStrategy>> GetByUserPagedAsync(int userId, PageOptions page, CancellationToken ct);
	Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(int userId, CancellationToken ct);
}
