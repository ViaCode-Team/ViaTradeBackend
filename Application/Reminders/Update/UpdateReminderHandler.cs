using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Reminders.Common.Abstractions;

namespace ViaTrade.Application.Reminders.Update;

public sealed class UpdateReminderHandler(IUserContext userContext, IReminderRepository reminderOperations)
	: IVoidCommandHandler<UpdateReminderCommand>
{
	public async ValueTask Handle(UpdateReminderCommand command, CancellationToken ct)
	{
		int rows = await reminderOperations.ExecuteUpdateForUserAsync(
			userContext.UserId,
			command.ReminderId,
			command.Text,
			command.RemindAt,
			ct
		);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
