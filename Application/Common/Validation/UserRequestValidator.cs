using System.Linq.Expressions;
using FluentValidation;

namespace ViaTrade.Application.Common.Validation;

public abstract class UserRequestValidator<T> : AbstractValidator<T>
{
	protected UserRequestValidator(Expression<Func<T, int>> userId)
	{
		RuleFor(userId).PositiveId();
	}
}
