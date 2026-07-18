using Application.Models.Statistic;
using Domain.Models.Sort;
using Domain.Models.TradeLogic;

namespace Application.Interfaces;

public interface ITradeResultsService
{
	Task<SignalStatisticReadModel> GetStrategyResultStatisticAsync(
		int userId,
		CancellationToken cancellationToken);

	Task<StrategyResultResponse> GetStrategyResultAsync(
		int userId,
		DateTime? startDate,
		DateTime? endDate,
		SignalSortRequest? sortRequest,
		CancellationToken cancellationToken);

	Task<StrategyResultResponse> GetStrategyResultByCodeAsync(
		int userId,
		string strategyName,
		string tradeCode,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken);
}
