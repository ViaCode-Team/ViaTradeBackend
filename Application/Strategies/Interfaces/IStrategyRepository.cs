using ViaTrade.Application.Strategies.Models;

namespace ViaTrade.Application.Strategies.Interfaces;

public interface IStrategyRepository
{
	Task<StrategyCountsDto?> FindStatisticsAsync(int userId, CancellationToken ct = default);
	Task<StrategyInstrumentLinkState?> FindInstrumentLinkStateAsync(
		int userId,
		int strategyId,
		int instrumentId,
		CancellationToken ct = default
	);
}
