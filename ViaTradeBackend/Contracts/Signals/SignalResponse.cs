namespace ViaTradeBackend.Contracts.Signals;

public record SignalResponse(
	int StrategyId,
	string StrategyName,
	int InstrumentId,
	string Symbol,
	int? Accuracy,
	DateTime Date,
	decimal ClosePrice,
	string Signal
);
