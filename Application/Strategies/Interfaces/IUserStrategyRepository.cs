using Application.Common.Interfaces.Repositories;
using Application.Trades.Models;
using Domain.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyRepository : IRepository<UserStrategy>
{
	Task<int> CountByUserAsync(int userId, CancellationToken ct);
	Task<List<SignalSourceDto>> ListSignalSourcesAsync(int userId, CancellationToken ct);
}
