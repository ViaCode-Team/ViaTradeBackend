using Mediator;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.DeleteDelivered;

public sealed class DeleteDeliveredRemindersHandler(IRepository<Reminder> reminderRepository)
	: ICommandHandler<DeleteDeliveredRemindersCommand, DeleteRemindersResult>
{
	public async ValueTask<DeleteRemindersResult> Handle(DeleteDeliveredRemindersCommand command, CancellationToken ct)
	{
		var count = await reminderRepository.ExecuteDeleteAsync(
			reminder => reminder.DeliveredAt != null && reminder.DeliveredAt <= command.DeliveredBefore,
			ct
		);

		return new DeleteRemindersResult(count);
	}
}
