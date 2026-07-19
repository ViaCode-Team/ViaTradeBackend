using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode>
{
	Task<PagedResult<UserStrategyTradeCode>> GetPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct);
}
