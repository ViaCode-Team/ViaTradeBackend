using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Update;

public sealed class UpdateReminderCommandValidator : UserRequestValidator<UpdateReminderCommand>
{
	public UpdateReminderCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.ReminderId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
