using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.UnlinkInstrument;

public sealed class UnlinkStrategyInstrumentCommandValidator : AbstractValidator<UnlinkStrategyInstrumentCommand>
{
	public UnlinkStrategyInstrumentCommandValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
