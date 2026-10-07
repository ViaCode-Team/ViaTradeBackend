using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.GetPage;

public sealed class GetRemindersPageQueryValidator : AbstractValidator<GetRemindersPageQuery>
{
	public GetRemindersPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.ReminderSearch).ValidSearch();
		RuleFor(request => request.ReminderSort).RequiredValid(new SortValidator<ReminderSort, ReminderSortField>());
	}
}
