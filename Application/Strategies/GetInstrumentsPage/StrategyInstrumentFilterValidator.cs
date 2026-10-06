using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed class StrategyInstrumentFilterValidator : AbstractValidator<StrategyInstrumentFilter>
{
	public StrategyInstrumentFilterValidator()
	{
		RuleFor(filter => filter.InstrumentIds)
			.MaxItems(StrategyInstrumentFilter.MaxInstrumentIds)
			.WithMessage("instrumentIds must contain no more than 100 items.");
		RuleForEach(filter => filter.InstrumentIds).GreaterThan(0);
	}
}
