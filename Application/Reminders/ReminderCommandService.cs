using Application.Common.Interfaces;
using Application.Reminders.Interfaces;
using Domain.Reminders.Entities;

namespace Application.Reminders;

public class ReminderCommandService(IReminderRepository reminderRepository, IUnitOfWork UoW) : IReminderCommandService
{
	public async Task CreateAsync(int userId, int tradeCodeId, string text, DateTime dateTime, CancellationToken ct)
	{
		var reminder = new Reminder
		{
			Text = text,
			DateTime = dateTime,
			TradeCodeId = tradeCodeId,
			UserId = userId,
		};

		await reminderRepository.AddAsync(reminder, ct);

		await UoW.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(int reminderId, int userId, string text, DateTime dateTime, CancellationToken ct)
	{
		int rows = await reminderRepository.UpdateForUserAsync(reminderId, userId, text, dateTime, ct);

		if (rows == 0)
			throw new KeyNotFoundException("Reminder not found.");
	}

	public async Task DeleteAsync(int reminderId, int userId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(x => x.Id == reminderId && x.UserId == userId, ct);

		if (rows == 0)
			throw new KeyNotFoundException("Reminder not found.");
	}

	public async Task DeleteAsync(int reminderId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(x => x.Id == reminderId, ct);

		if (rows == 0)
			throw new KeyNotFoundException("Reminder not found.");
	}
}
