namespace ViaTrade.Application.Signals.Common;

public record SignalSource(
	int StrategyId,
	string StrategyName,
	string DisplayName,
	int InstrumentId,
	string Symbol,
	int? Accuracy
);
