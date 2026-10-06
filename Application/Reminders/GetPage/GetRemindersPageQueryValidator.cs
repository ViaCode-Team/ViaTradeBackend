using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.GetPage;

public sealed class GetRemindersPageQueryValidator : UserRequestValidator<GetRemindersPageQuery>
{
	public GetRemindersPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.ReminderSearch).ValidSearch();
		RuleFor(request => request.ReminderSort).RequiredValid(new SortValidator<ReminderSort, ReminderSortField>());
	}
}
