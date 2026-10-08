namespace ViaTrade.Application.Signals.Common;

public record SignalResult(
	int StrategyId,
	string StrategyName,
	string DisplayName,
	int InstrumentId,
	string Ticker,
	int? Accuracy,
	DateTime Date,
	decimal ClosePrice,
	string Signal
);
