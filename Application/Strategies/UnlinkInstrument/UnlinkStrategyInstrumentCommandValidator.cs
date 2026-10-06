using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.UnlinkInstrument;

public sealed class UnlinkStrategyInstrumentCommandValidator : UserRequestValidator<UnlinkStrategyInstrumentCommand>
{
	public UnlinkStrategyInstrumentCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
