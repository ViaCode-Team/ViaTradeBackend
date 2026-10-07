using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Strategies.Common.Abstractions;

namespace ViaTrade.Application.Strategies.GetStatistics;

public sealed class GetStrategyStatisticsHandler(IUserContext userContext, IStrategyRepository strategyStatistics)
	: IQueryHandler<GetStrategyStatisticsQuery, StrategyStatisticsResult>
{
	public async Task<StrategyStatisticsResult> HandleAsync(GetStrategyStatisticsQuery query, CancellationToken ct)
	{
		var counts = await strategyStatistics.FindStatisticsAsync(userContext.UserId, ct);
		if (counts == null)
			throw new NotFoundException("User not found.", "user_not_found");

		long unsubscribedStrategiesCount = counts.TotalStrategiesCount - counts.SubscribedStrategiesCount;
		if (unsubscribedStrategiesCount < 0)
		{
			throw new DataIntegrityException(
				$"Subscribed strategy count exceeds total strategy count. "
					+ $"UserId={userContext.UserId}, "
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
