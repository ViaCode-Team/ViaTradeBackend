using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode>
{
	Task<PageResult<UserStrategyTradeCode>> GetPagedAsync(int userId, PageOptions page, CancellationToken ct = default);
}
