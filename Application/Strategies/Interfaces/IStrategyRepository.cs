using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Interfaces;

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
