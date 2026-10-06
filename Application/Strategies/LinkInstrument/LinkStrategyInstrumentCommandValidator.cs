using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.LinkInstrument;

public sealed class LinkStrategyInstrumentCommandValidator : UserRequestValidator<LinkStrategyInstrumentCommand>
{
	public LinkStrategyInstrumentCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
