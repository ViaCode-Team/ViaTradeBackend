using Application.Common.Queries;
using Application.Common.Specifications;
using Application.Reminders.Interfaces;
using Application.Reminders.Queries;
using Domain.Reminders.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class ReminderEfRepository(AppDbContext context)
	: GenericEfRepository<Reminder>(context), IReminderRepository
{
	public async Task<IEnumerable<Reminder>> GetDueRemindersAsync(CancellationToken ct)
	{
		return await _dbSet.Where(r => r.DateTime <= DateTime.Now).ToListAsync(ct);
	}

	public async Task<PageResult<Reminder>> GetByUserPagedAsync(int userId, PageOptions page, ReminderSort sort, CancellationToken ct)
	{
		var spec = new ReminderQuerySpecification(userId, null, sort);
		return await GetPagedAsync(spec, page, ct);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, ct);
	}

	public async Task<PageResult<Reminder>> GetByUserAndTradeCodePagedAsync(
		int userId, int tradeCodeId, PageOptions page, ReminderSort sort, CancellationToken ct)
	{
		var spec = new ReminderQuerySpecification(userId, tradeCodeId, sort);
		return await GetPagedAsync(spec, page, ct);
	}

	public async Task<int> UpdateForUserAsync(int reminderId, int userId, string text, DateTime dateTime, CancellationToken ct)
	{
		return await _dbSet
			.Where(r => r.Id == reminderId && r.UserId == userId)
			.ExecuteUpdateAsync(s => s
				.SetProperty(r => r.Text, text)
				.SetProperty(r => r.DateTime, dateTime),
				ct);
	}
}
