using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.DeleteDelivered;

public sealed class DeleteDeliveredRemindersHandler(IRepository<Reminder> reminderRepository)
	: ICommandHandler<DeleteDeliveredRemindersCommand, DeleteRemindersResult>
{
	public async Task<DeleteRemindersResult> HandleAsync(
		DeleteDeliveredRemindersCommand command,
		CancellationToken ct = default
	)
	{
		var count = await reminderRepository.ExecuteDeleteAsync(
			reminder => reminder.DeliveredAt != null && reminder.DeliveredAt <= command.DeliveredBefore,
			ct
		);

		return new DeleteRemindersResult(count);
	}
}
