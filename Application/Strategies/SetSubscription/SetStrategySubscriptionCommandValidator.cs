using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.SetSubscription;

public sealed class SetStrategySubscriptionCommandValidator : AbstractValidator<SetStrategySubscriptionCommand>
{
	public SetStrategySubscriptionCommandValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.IsSubscribed).NotNull();
	}
}
