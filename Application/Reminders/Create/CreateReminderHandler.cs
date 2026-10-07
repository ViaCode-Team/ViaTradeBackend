using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Create;

public sealed class CreateReminderHandler(
	IUserContext userContext,
	IRepository<Reminder> reminderRepository,
	IUnitOfWork uow,
	IOptions<ReminderLimitsSettings> reminderLimitsOptions
) : ICommandHandler<CreateReminderCommand, ReminderResult>
{
	public async Task<ReminderResult> HandleAsync(CreateReminderCommand command, CancellationToken ct)
	{
		int reminderCount = await reminderRepository.CountAsync(reminder => reminder.UserId == userContext.UserId, ct);
		if (reminderCount >= reminderLimitsOptions.Value.MaxRemindersPerUser)
			throw new BusinessRuleException(
				"The maximum number of reminders has been reached.",
				"reminder_limit_exceeded"
			);

		var reminder = new Reminder
		{
			Text = command.Text,
			RemindAt = command.RemindAt,
			InstrumentId = command.InstrumentId,
			UserId = userContext.UserId,
		};

		reminderRepository.Add(reminder);
		await uow.SaveChangesAsync(ct);

		return new ReminderResult(reminder.Id, reminder.Text, reminder.RemindAt, null, reminder.DeliveredAt);
	}
}
