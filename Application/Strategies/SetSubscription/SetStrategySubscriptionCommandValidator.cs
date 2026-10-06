using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.SetSubscription;

public sealed class SetStrategySubscriptionCommandValidator : UserRequestValidator<SetStrategySubscriptionCommand>
{
	public SetStrategySubscriptionCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.IsSubscribed).NotNull();
	}
}
