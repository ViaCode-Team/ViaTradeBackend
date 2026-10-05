using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Delete;

public sealed class DeleteReminderHandler(IRepository<Reminder> reminderRepository)
	: ICommandHandler<DeleteReminderCommand>
{
	public async Task HandleAsync(DeleteReminderCommand command, CancellationToken ct = default)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(x => x.Id == command.ReminderId && x.UserId == command.UserId, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
