using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Create;

public sealed class CreateReminderCommandValidator : AbstractValidator<CreateReminderCommand>
{
	public CreateReminderCommandValidator()
	{
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
