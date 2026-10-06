using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Get;

public sealed class GetReminderQueryValidator : UserRequestValidator<GetReminderQuery>
{
	public GetReminderQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.ReminderId).PositiveId();
	}
}
