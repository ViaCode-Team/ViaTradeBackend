using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Delete;

public sealed class DeleteReminderCommandValidator : AbstractValidator<DeleteReminderCommand>
{
	public DeleteReminderCommandValidator()
	{
		RuleFor(request => request.ReminderId).PositiveId();
	}
}
