using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserTradeStrategyRepository : IRepository<UserTradeStrategy>
{
	Task<PagedResult<UserTradeStrategy>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken);
	Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);
}
