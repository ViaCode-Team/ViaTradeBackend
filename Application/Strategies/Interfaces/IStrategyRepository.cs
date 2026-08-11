using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Strategies.Models;
using Domain.Entities;

namespace Application.Strategies.Interfaces;

public interface IStrategyRepository : IRepository<Strategy>
{
	Task<StrategyCountsDto?> FindStatisticsAsync(int userId, CancellationToken ct = default);
	Task<Dictionary<string, int?>> GetAccuracyMapAsync(CancellationToken ct = default);
	Task<StrategySubscriptionDto?> FindSubscriptionAsync(int userId, int strategyId, CancellationToken ct = default);
	Task<StrategyInstrumentLinkState?> FindInstrumentLinkStateAsync(
		int userId,
		int strategyId,
		int instrumentId,
		CancellationToken ct = default
	);
	Task<int?> FindAccuracyByNameAsync(string name, CancellationToken ct = default);
	Task<PageResult<StrategySubscriptionDto>> GetPageAsync(
		int userId,
		IQuerySpecification<Strategy> spec,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
}
