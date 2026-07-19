using Application.Common.Interfaces.Repositories;
using Application.Common.Queries;
using Application.Reminders.Queries;
using Domain.Reminders.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
	Task<IEnumerable<Reminder>> GetDueRemindersAsync(CancellationToken ct = default);
	Task<PageResult<Reminder>> GetByUserPagedAsync(
		int userId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct = default
	);
	Task<PageResult<Reminder>> GetByUserAndTradeCodePagedAsync(
		int userId,
		int tradeCodeId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct = default
	);
	Task<int> CountByUserAsync(int userId, CancellationToken ct = default);
	Task<int> UpdateForUserAsync(
		int reminderId,
		int userId,
		string text,
		DateTime dateTime,
		CancellationToken ct = default
	);
}
