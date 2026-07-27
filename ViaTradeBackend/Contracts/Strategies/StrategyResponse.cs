using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Strategies;

public record StrategyResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(255)] string Name,
	string? Description,
	int? Accuracy,
	string? SignalFrequency,
	string? InvestmentHorizon,
	string? LogicDescription,
	string? UsageDescription,
	string? LimitationsDescription,
	bool IsActive
);
