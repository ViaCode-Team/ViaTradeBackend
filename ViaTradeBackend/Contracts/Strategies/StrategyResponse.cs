namespace ViaTradeBackend.Contracts.Strategies;

public record StrategyResponse(
	int Id,
	string Name,
	string? Description,
	int? Accuracy,
	string? SignalFrequency,
	string? InvestmentHorizon,
	string? LogicDesc,
	string? UseDesc,
	string? LimitDesc,
	bool IsActive
);
