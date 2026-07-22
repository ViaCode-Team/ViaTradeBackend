using Application.Trades.Models;

namespace Application.Interfaces;

public interface ITradeResultsService
{
	Task<SignalStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);

	Task<StrategyResults> GetStrategyResultsAsync(
		int userId,
		DateTime? startDate,
		DateTime? endDate,
		SignalSort signalSort,
		CancellationToken ct
	);

	Task<StrategyResults> GetStrategyTradeCodeResultsAsync(
		int userId,
		string strategyName,
		string tradeCode,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken ct
	);
}
