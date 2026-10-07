using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Get;

public sealed class GetReminderQueryValidator : AbstractValidator<GetReminderQuery>
{
	public GetReminderQueryValidator()
	{
		RuleFor(request => request.ReminderId).PositiveId();
	}
}
