using Domain.Entities.DataBase;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface IUserTradeStrategyRepository : IRepository<UserTradeStrategy>
{
	Task<PagedResult<UserTradeStrategy>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken);
	Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);
}
