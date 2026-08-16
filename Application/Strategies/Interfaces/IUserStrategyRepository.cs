using Application.Common.Interfaces.Repositories;
using Application.Trades.Models;
using Domain.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyRepository : IRepository<UserStrategy>
{
	Task<int> CountByUserAsync(int userId, CancellationToken ct);

	/// <summary>
	/// Removes only the subscription and preserves user strategy instrument configuration.
	/// </summary>
	Task ExecuteUnsubscribeAsync(int userId, int strategyId, CancellationToken ct);

	Task<List<SignalSourceDto>> ListSignalSourcesAsync(int userId, CancellationToken ct);
}
