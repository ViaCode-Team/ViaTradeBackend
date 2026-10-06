using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.MarkDelivered;

public sealed class MarkReminderDeliveredCommandValidator : UserRequestValidator<MarkReminderDeliveredCommand>
{
	public MarkReminderDeliveredCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.ReminderId).PositiveId();
	}
}
