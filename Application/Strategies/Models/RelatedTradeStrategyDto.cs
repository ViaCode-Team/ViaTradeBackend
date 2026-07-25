namespace Application.Strategies.Models;

public record RelatedTradeStrategyDto(
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
