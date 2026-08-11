using Application.Common.Models;
using Application.Instruments.Models;
using Application.Notes.Models;
using Application.Strategies.Models;

namespace Application.Strategies.Interfaces;

public interface IStrategyQueryService
{
	Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<PageResult<StrategySubscriptionDto>> GetPageAsync(
		int userId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<StrategySubscriptionDto> GetAsync(int userId, int strategyId, CancellationToken ct);
	Task<PageResult<StrategySubscriptionDto>> GetPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<PageResult<RelatedInstrumentDto>> GetInstrumentsByStrategyPageAsync(
		int userId,
		int strategyId,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
