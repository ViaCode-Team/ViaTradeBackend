using Application.Statistics.Models;
using Application.Trades.Models;
using Application.Trades.Queries;

namespace Application.Interfaces;

public interface ITradeResultsService
{
	Task<SignalStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);

	Task<StrategyResults> GetAsync(
		int userId,
		DateTime? startDate,
		DateTime? endDate,
		SignalSort sort,
		CancellationToken ct
	);

	Task<StrategyResults> GetAsync(
		int userId,
		string strategyName,
		string tradeCode,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken ct
	);
}
