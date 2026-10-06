namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed record StrategyInstrumentFilter(List<int>? InstrumentIds)
{
	public const int MaxInstrumentIds = 100;
}
