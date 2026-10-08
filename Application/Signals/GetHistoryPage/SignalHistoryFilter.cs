using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Signals.GetHistoryPage;

public sealed class SignalHistoryFilter
{
	public required int StrategyId { get; set; }

	public required int InstrumentId { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public List<TradeSignal>? Signals { get; set; }
}
