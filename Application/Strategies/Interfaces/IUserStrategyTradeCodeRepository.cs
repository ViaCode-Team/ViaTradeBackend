using Application.Common.Interfaces.Repositories;
using Application.Common.Queries;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode>
{
	Task<PageResult<UserStrategyTradeCode>> GetPagedAsync(int userId, PageOptions page, CancellationToken ct = default);
}
