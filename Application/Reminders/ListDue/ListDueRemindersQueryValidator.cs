using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed class ListDueRemindersQueryValidator : AbstractValidator<ListDueRemindersQuery>
{
	public ListDueRemindersQueryValidator()
	{
		RuleFor(request => request.Limit).PositiveId();
	}
}
