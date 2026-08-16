namespace Application.Trades.Models;

public record SignalSourceDto(
	int StrategyId,
	string StrategyName,
	string DisplayName,
	int InstrumentId,
	string Symbol,
	int? Accuracy
);
