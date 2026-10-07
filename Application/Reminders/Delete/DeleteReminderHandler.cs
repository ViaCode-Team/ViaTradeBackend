using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Delete;

public sealed class DeleteReminderHandler(IUserContext userContext, IRepository<Reminder> reminderRepository)
	: IVoidCommandHandler<DeleteReminderCommand>
{
	public async ValueTask Handle(DeleteReminderCommand command, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(
			x => x.Id == command.ReminderId && x.UserId == userContext.UserId,
			ct
		);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
