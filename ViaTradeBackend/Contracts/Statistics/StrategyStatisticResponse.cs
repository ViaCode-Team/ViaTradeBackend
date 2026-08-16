using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Statistics;

public record StrategyStatisticResponse(
	[Range(0, long.MaxValue)] long TotalStrategiesCount,
	[Range(0, long.MaxValue)] long SubscribedStrategiesCount,
	[Range(0, long.MaxValue)] long UnsubscribedStrategiesCount
);
