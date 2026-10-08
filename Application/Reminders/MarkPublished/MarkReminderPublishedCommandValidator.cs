using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.MarkPublished;

public sealed class MarkReminderPublishedCommandValidator : UserRequestValidator<MarkReminderPublishedCommand>
{
	public MarkReminderPublishedCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.ReminderId).PositiveId();
	}
}
