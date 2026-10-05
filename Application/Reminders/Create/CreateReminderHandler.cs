using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Create;

public sealed class CreateReminderHandler(
	IRepository<Reminder> reminderRepository,
	IUnitOfWork uow,
	IOptions<ReminderLimitsSettings> reminderLimitsOptions
) : ICommandHandler<CreateReminderCommand, CreateReminderResult>
{
	public async Task<CreateReminderResult> HandleAsync(CreateReminderCommand command, CancellationToken ct = default)
	{
		int reminderCount = await reminderRepository.CountAsync(reminder => reminder.UserId == command.UserId, ct);
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
			UserId = command.UserId,
		};

		reminderRepository.Add(reminder);
		await uow.SaveChangesAsync(ct);

		return new CreateReminderResult(reminder.Id, reminder.Text, reminder.RemindAt, reminder.DeliveredAt);
	}
}
