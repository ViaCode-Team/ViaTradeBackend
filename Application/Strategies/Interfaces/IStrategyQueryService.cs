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
	Task<PageResult<Strategy>> GetPageSearchAsync(
		SearchFilter strategyFilter,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<Strategy> GetAsync(int userId, int strategyId, CancellationToken ct);
	Task<PageResult<Strategy>> GetPageByInstrumentAsync(
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
		StrategyInstrumentFilter instrumentFilter,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
