using Application.Common.Models;
using Application.Common.Specifications;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Domain.Reminders.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ReminderEfRepository(AppDbContext context) : GenericEfRepository<Reminder>(context), IReminderRepository
{
	public async Task<IReadOnlyList<Reminder>> ListDueAsync(CancellationToken ct)
	{
		return await _dbSet.Where(reminder => reminder.DateTime <= DateTime.UtcNow).ToListAsync(ct);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, ct);
	}

	public async Task<int> ExecuteUpdateForUserAsync(
		int userId,
		int reminderId,
		string text,
		DateTime dateTime,
		CancellationToken ct
	)
	{
		var affectedRows = await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(r => r.Id == reminderId && r.UserId == userId)
				.ExecuteUpdateAsync(
					s => s.SetProperty(r => r.TextRemind, text).SetProperty(r => r.DateTime, dateTime),
					ct
				)
		);

		return affectedRows;
	}
}
