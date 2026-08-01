namespace Application.Trades.Models;

public record SignalSourceDto(int StrategyId, string StrategyName, int InstrumentId, string Symbol, int? Accuracy);
