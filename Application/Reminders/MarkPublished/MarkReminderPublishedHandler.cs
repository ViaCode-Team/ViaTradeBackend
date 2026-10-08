using Mediator;
using ViaTrade.Application.Reminders.Common.Abstractions;

namespace ViaTrade.Application.Reminders.MarkPublished;

public sealed class MarkReminderPublishedHandler(IReminderRepository reminderOperations)
	: ICommandHandler<MarkReminderPublishedCommand, PublishReminderResult>
{
	public async ValueTask<PublishReminderResult> Handle(MarkReminderPublishedCommand command, CancellationToken ct)
	{
		int rows = await reminderOperations.ExecuteMarkPublishedAsync(command.UserId, command.ReminderId, ct);

		return new PublishReminderResult(rows > 0);
	}
}
