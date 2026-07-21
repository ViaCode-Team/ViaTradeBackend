namespace ViaTradeBackend.Contracts.Statistics;

public record StrategyStatisticResponse(
	long TotalStrategiesCount,
	long ActiveStrategiesCount,
	long NotLinkedStrategiesCount
);
