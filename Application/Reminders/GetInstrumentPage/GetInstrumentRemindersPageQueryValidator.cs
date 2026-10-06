using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.GetInstrumentPage;

public sealed class GetInstrumentRemindersPageQueryValidator : UserRequestValidator<GetInstrumentRemindersPageQuery>
{
	public GetInstrumentRemindersPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.ReminderSearch).ValidSearch();
		RuleFor(request => request.ReminderSort).RequiredValid(new SortValidator<ReminderSort, ReminderSortField>());
	}
}
