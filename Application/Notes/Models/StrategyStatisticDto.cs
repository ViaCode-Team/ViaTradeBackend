namespace ViaTrade.Application.Notes.Models;

public record StrategyStatisticDto(
	long TotalStrategiesCount,
	long SubscribedStrategiesCount,
	long UnsubscribedStrategiesCount
);
