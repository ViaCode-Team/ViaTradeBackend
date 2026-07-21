using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Reminders.Interfaces;
using Application.TradeCodes.Interfaces;
using Domain.Reminders.Entities;

namespace Application.Reminders;

public class ReminderCommandService(
	IReminderRepository reminderRepository,
	ITradeCodeRepository tradeCodeRepository,
	IUnitOfWork uow
) : IReminderCommandService
{
	public async Task CreateAsync(int userId, int tradeCodeId, string text, DateTime dateTime, CancellationToken ct)
	{
		var reminder = new Reminder
		{
			TextRemind = text,
			DateTime = dateTime,
			TradeCodeId = tradeCodeId,
			UserId = userId,
		};

		await reminderRepository.AddAsync(reminder, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(int reminderId, int userId, string text, DateTime dateTime, CancellationToken ct)
	{
		int rows = await reminderRepository.UpdateForUserAsync(reminderId, userId, text, dateTime, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}

	public async Task DeleteAsync(int reminderId, int userId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(x => x.Id == reminderId && x.UserId == userId, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}

	public async Task DeleteAsync(int reminderId, CancellationToken ct)
	{
		int rows = await reminderRepository.ExecuteDeleteAsync(x => x.Id == reminderId, ct);

		if (rows == 0)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");
	}
}
