using System.Linq.Expressions;
using FluentValidation;

namespace ViaTrade.Application.Common.Validation;

public abstract class DateRangeValidator<T, TDate> : AbstractValidator<T>
	where TDate : struct, IComparable<TDate>
{
	protected DateRangeValidator(Expression<Func<T, TDate?>> startDate, Expression<Func<T, TDate?>> endDate)
	{
		var getEndDate = endDate.Compile();
		RuleFor(startDate)
			.Must(
				(filter, start) =>
				{
					var end = getEndDate(filter);
					return !start.HasValue || !end.HasValue || start.Value.CompareTo(end.Value) <= 0;
				}
			)
			.WithMessage("startDate must be less than or equal to endDate.");
	}
}
