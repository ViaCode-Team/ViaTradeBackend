using ViaTrade.Application.Strategies.GetStatistics;
using ViaTrade.Application.Strategies.LinkInstrument;

namespace ViaTrade.Application.Strategies.Common.Abstractions;

public interface IStrategyRepository
{
	Task<StrategyCounts?> FindStatisticsAsync(int userId, CancellationToken ct = default);
	Task<StrategyInstrumentLinkState?> FindInstrumentLinkStateAsync(
		int userId,
		int strategyId,
		int instrumentId,
		CancellationToken ct = default
	);
}
