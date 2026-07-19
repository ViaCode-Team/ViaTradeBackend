using Application.Common.Queries;
using Application.Reminders.Models;
using Application.Reminders.Queries;
using Domain.Reminders.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderQueryService
{
	Task<ReminderStatistics> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<IEnumerable<Reminder>> GetAsync(CancellationToken ct);
	Task<Reminder> GetAsync(int reminderId, int userId, CancellationToken ct);
	Task<PageResult<Reminder>> GetAsync(
		int userId,
		int tradeCodeId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct);
	Task<PageResult<Reminder>> GetAsync(
		int userId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct);
}
