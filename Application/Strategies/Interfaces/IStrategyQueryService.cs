using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Strategies.Models;

namespace ViaTrade.Application.Strategies.Interfaces;

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
	Task<PageResult<StrategySubscriptionDto>> GetPageSearchAsync(
		int userId,
		SearchFilter strategyFilter,
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
		StrategyInstrumentFilter instrumentFilter,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
