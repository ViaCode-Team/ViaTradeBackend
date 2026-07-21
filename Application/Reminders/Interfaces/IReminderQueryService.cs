using Application.Common.Models;
using Application.Reminders.Models;
using Domain.Reminders.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderQueryService
{
	Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<IEnumerable<Reminder>> GetAsync(CancellationToken ct);
	Task<Reminder> GetAsync(int reminderId, int userId, CancellationToken ct);
	Task<PageResult<Reminder>> GetAsync(
		int userId,
		int tradeCodeId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct
	);
	Task<PageResult<Reminder>> GetAsync(int userId, PageOptions page, ReminderSort sort, CancellationToken ct);
}
