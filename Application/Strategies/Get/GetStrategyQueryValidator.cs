using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.Get;

public sealed class GetStrategyQueryValidator : UserRequestValidator<GetStrategyQuery>
{
	public GetStrategyQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
	}
}
