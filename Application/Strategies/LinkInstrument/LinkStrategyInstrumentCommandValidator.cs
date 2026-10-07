using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.LinkInstrument;

public sealed class LinkStrategyInstrumentCommandValidator : AbstractValidator<LinkStrategyInstrumentCommand>
{
	public LinkStrategyInstrumentCommandValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
