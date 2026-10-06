using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Delete;

public sealed class DeleteReminderCommandValidator : UserRequestValidator<DeleteReminderCommand>
{
	public DeleteReminderCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.ReminderId).PositiveId();
	}
}
