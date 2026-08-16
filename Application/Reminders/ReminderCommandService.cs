using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders;

public class ReminderCommandService(
	IReminderRepository reminderRepository,
	IUnitOfWork uow,
	IOptions<ReminderLimitsSettings> reminderLimitsOptions
) : IReminderCommandService
{
	public async Task<Reminder> CreateAsync(
		int userId,
		int instrumentId,
		string text,
		DateTime remindAt,
		CancellationToken ct
	)
	{
		int reminderCount = await reminderRepository.CountByUserAsync(userId, ct);
		if (reminderCount >= reminderLimitsOptions.Value.MaxRemindersPerUser)
			throw new BusinessRuleException(
				"The maximum number of reminders has been reached.",
				"reminder_limit_exceeded"
			);

		var reminder = new Reminder
		{
			Text = text,
			RemindAt = remindAt,
			InstrumentId = instrumentId,
			UserId = userId,
		};

		await reminderRepository.AddAsync(reminder, ct);
		await uow.SaveChangesAsync(ct);

		return reminder;
	}

	public async Task UpdateAsync(int userId, int reminderId, string text, DateTime remindAt, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteUpdateForUserAsync(userId, reminderId, text, remindAt, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}

	public async Task DeleteAsync(int userId, int reminderId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(x => x.Id == reminderId && x.UserId == userId, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}

	public async Task<bool> MarkPublishedAsync(int reminderId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteMarkPublishedAsync(reminderId, ct);

		return rows > 0;
	}

	public async Task MarkDeliveredAsync(int userId, int reminderId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteMarkDeliveredForUserAsync(userId, reminderId, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}

	public Task<int> DeleteDeliveredBeforeAsync(DateTime deliveredBefore, CancellationToken ct)
	{
		return reminderRepository.ExecuteDeleteDeliveredBeforeAsync(deliveredBefore, ct);
	}
}
