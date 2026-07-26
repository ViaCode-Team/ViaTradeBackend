using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Trades.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserTradeStrategyRepository : IRepository<UserTradeStrategy>
{
	Task<int> CountByUserAsync(int userId, CancellationToken ct);
	Task<List<SignalSourceDto>> ListSignalSourcesAsync(int userId, CancellationToken ct);
}
