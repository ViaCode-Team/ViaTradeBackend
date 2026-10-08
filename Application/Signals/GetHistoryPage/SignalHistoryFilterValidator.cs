using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Signals.GetHistoryPage;

public sealed class SignalHistoryFilterValidator : DateRangeValidator<SignalHistoryFilter, DateTime>
{
	public SignalHistoryFilterValidator()
		: base(filter => filter.StartDate, filter => filter.EndDate)
	{
		RuleFor(filter => filter.StrategyId).PositiveId();
		RuleFor(filter => filter.InstrumentId).PositiveId();
	}
}
