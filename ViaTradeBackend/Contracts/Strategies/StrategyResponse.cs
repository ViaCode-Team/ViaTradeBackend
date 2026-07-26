namespace ViaTradeBackend.Contracts.Strategies;

public record StrategyResponse(
	int Id,
	string Name,
	string? Description,
	int? Accuracy,
	string? SignalFrequency,
	string? InvestmentHorizon,
	string? LogicDescription,
	string? UsageDescription,
	string? LimitationsDescription,
	bool IsActive
);
