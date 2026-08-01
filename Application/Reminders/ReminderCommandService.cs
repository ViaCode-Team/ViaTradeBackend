using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Reminders.Interfaces;
using Domain.Entities;

namespace Application.Reminders;

public class ReminderCommandService(IReminderRepository reminderRepository, IUnitOfWork uow) : IReminderCommandService
{
	public async Task<Reminder> CreateAsync(
		int userId,
		int instrumentId,
		string text,
		DateTime remindAt,
		CancellationToken ct
	)
	{
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

	public async Task DeleteDueAsync(int reminderId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(
			x => x.Id == reminderId && x.RemindAt <= DateTime.UtcNow,
			ct
		);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
