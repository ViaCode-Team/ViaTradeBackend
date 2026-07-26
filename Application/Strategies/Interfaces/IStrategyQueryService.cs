using Application.Common.Models;
using Application.Notes.Models;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IStrategyQueryService
{
	Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<PageResult<TradeStrategy>> GetPageAsync(
		int userId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<TradeStrategy> GetAsync(int userId, int strategyId, CancellationToken ct);
	Task<PageResult<TradeStrategy>> GetPageByInstrumentAsync(
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
