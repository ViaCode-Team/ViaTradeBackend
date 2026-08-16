using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Models;

public sealed class LatestSignalFilter
{
	public List<TradeSignal>? Signals { get; set; }
}
