using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Statistics;

public record StrategyStatisticResponse(
	[Range(0, long.MaxValue)] long TotalStrategiesCount,
	[Range(0, long.MaxValue)] long SubscribedStrategiesCount,
	[Range(0, long.MaxValue)] long UnsubscribedStrategiesCount
);
