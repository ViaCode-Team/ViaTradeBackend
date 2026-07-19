using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Domain.Reminds.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
	Task<IEnumerable<Reminder>> GetActualReminder(CancellationToken ct);
	Task<PagedResult<Reminder>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, ReminderSortRequest sortRequest, CancellationToken ct = default);
	Task<PagedResult<Reminder>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, ReminderSortRequest sortRequest, CancellationToken ct = default);
	Task<int> CountByUserAsync(int userId, CancellationToken ct);
	Task<int> ExecuteUpdateUserRemindAsync(int remindId, int userId, string textRemind, DateTime dateTime, CancellationToken ct = default);
}
