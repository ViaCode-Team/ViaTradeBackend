using Domain.Strategies.Entities;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode>
{
	Task<PagedResult<UserStrategyTradeCode>> GetPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
}
