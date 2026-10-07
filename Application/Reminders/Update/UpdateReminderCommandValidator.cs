using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Update;

public sealed class UpdateReminderCommandValidator : AbstractValidator<UpdateReminderCommand>
{
	public UpdateReminderCommandValidator()
	{
		RuleFor(request => request.ReminderId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
