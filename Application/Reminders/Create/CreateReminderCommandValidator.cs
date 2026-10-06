using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.Create;

public sealed class CreateReminderCommandValidator : UserRequestValidator<CreateReminderCommand>
{
	public CreateReminderCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
