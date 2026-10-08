using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Signals.GetLatestPage;

public sealed class LatestSignalFilter
{
	public List<TradeSignal>? Signals { get; set; }
}
