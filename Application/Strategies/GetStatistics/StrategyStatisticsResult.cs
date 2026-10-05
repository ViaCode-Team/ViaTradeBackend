namespace ViaTrade.Application.Strategies.GetStatistics;

public record StrategyStatisticsResult(
	long TotalStrategiesCount,
	long SubscribedStrategiesCount,
	long UnsubscribedStrategiesCount
);
