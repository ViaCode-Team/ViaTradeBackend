using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Reminders.Models;
using Domain.Reminds.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderQueryService
{
	Task<RemindStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<IEnumerable<Reminder>> GetAsync(CancellationToken ct);
	Task<Reminder> GetAsync(int remindId, int userId, CancellationToken ct);
	Task<PagedResult<Reminder>> GetAsync(
		int userId,
		int tradeCodeId,
		PaginationRequest paginationRequest,
		ReminderSortRequest sortRequest,
		CancellationToken ct);
	Task<PagedResult<Reminder>> GetAsync(
		int userId,
		PaginationRequest paginationRequest,
		ReminderSortRequest sortRequest,
		CancellationToken ct);
}
