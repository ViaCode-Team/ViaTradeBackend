using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Strategies.Common.Abstractions;

namespace ViaTrade.Application.Strategies.GetStatistics;

public sealed class GetStrategyStatisticsHandler(IStrategyRepository strategyStatistics)
	: IQueryHandler<GetStrategyStatisticsQuery, StrategyStatisticsResult>
{
	public async Task<StrategyStatisticsResult> HandleAsync(
		GetStrategyStatisticsQuery query,
		CancellationToken ct = default
	)
	{
		var counts = await strategyStatistics.FindStatisticsAsync(query.UserId, ct);
		if (counts == null)
			throw new NotFoundException("User not found.", "user_not_found");

		long unsubscribedStrategiesCount = counts.TotalStrategiesCount - counts.SubscribedStrategiesCount;
		if (unsubscribedStrategiesCount < 0)
		{
			throw new DataIntegrityException(
				$"Subscribed strategy count exceeds total strategy count. "
					+ $"UserId={query.UserId}, "
					+ $"Total={counts.TotalStrategiesCount}, "
					+ $"Subscribed={counts.SubscribedStrategiesCount}."
			);
		}

		return new StrategyStatisticsResult(
			counts.TotalStrategiesCount,
			counts.SubscribedStrategiesCount,
			unsubscribedStrategiesCount
		);
	}
}
