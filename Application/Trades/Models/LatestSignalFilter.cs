using Domain.Enums;

namespace Application.Trades.Models;

public sealed class LatestSignalFilter
{
	public List<TradeSignal>? Signals { get; set; }
}
