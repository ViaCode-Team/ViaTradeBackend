using Application.Common.Models.Sort;
using Application.Statistics.Models;
using Domain.Trades.Entities;

namespace Application.Interfaces;

public interface ITradeResultsService
{
	Task<SignalStatisticReadModel> GetStatisticsAsync(
		int userId,
		CancellationToken ct);

	Task<StrategyResultResponse> GetAsync(
		int userId,
		DateTime? startDate,
		DateTime? endDate,
		SignalSortRequest sortRequest,
		CancellationToken ct);

	Task<StrategyResultResponse> GetAsync(
		int userId,
		string strategyName,
		string tradeCode,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken ct);
}
