using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Reminders.Common.Abstractions;

namespace ViaTrade.Application.Reminders.Update;

public sealed class UpdateReminderHandler(IReminderRepository reminderOperations)
	: ICommandHandler<UpdateReminderCommand>
{
	public async Task HandleAsync(UpdateReminderCommand command, CancellationToken ct = default)
	{
		int rows = await reminderOperations.ExecuteUpdateForUserAsync(
			command.UserId,
			command.ReminderId,
			command.Text,
			command.RemindAt,
			ct
		);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
