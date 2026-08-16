namespace ViaTrade.Application.Trades.Models;

public record SignalDto(
	int StrategyId,
	string StrategyName,
	string DisplayName,
	int InstrumentId,
	string Symbol,
	int? Accuracy,
	DateTime Date,
	decimal ClosePrice,
	string Signal
);
