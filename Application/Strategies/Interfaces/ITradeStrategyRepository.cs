using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Strategies.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface ITradeStrategyRepository : IRepository<TradeStrategy>
{
	Task<StrategyCountsDto> GetStatisticsAsync(int userId, CancellationToken ct = default);
	Task<Dictionary<string, int?>> GetAccuracyMapAsync(CancellationToken ct = default);
	Task<int?> FindAccuracyByNameAsync(string name, CancellationToken ct = default);
	Task<PageResult<TradeStrategy>> GetPageAsync(
		int userId,
		IQuerySpecification<TradeStrategy> spec,
		PageOptions page,
		CancellationToken ct = default
	);
}
