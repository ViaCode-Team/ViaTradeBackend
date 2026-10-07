using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.Get;

public sealed class GetStrategyQueryValidator : AbstractValidator<GetStrategyQuery>
{
	public GetStrategyQueryValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
	}
}
