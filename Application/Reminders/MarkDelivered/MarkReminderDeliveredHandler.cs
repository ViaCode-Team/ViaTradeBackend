using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Reminders.Common.Abstractions;

namespace ViaTrade.Application.Reminders.MarkDelivered;

public sealed class MarkReminderDeliveredHandler(IReminderRepository reminderOperations)
	: IVoidCommandHandler<MarkReminderDeliveredCommand>
{
	public async ValueTask Handle(MarkReminderDeliveredCommand command, CancellationToken ct)
	{
		int rows = await reminderOperations.ExecuteMarkDeliveredForUserAsync(command.UserId, command.ReminderId, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
