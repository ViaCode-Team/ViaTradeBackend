namespace Application.Notes.Models;

public record StrategyStatisticDto(
	long TotalStrategiesCount,
	long ActiveStrategiesCount,
	long NotLinkedStrategiesCount
);
