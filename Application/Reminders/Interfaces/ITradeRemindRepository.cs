using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Domain.Reminds.Entities;

namespace Application.Reminds.Interfaces;

public interface ITradeRemindRepository : IRepository<Reminder>
{
	Task<IEnumerable<Reminder>> GetActualRemind(CancellationToken ct);
	Task<PagedResult<Reminder>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken ct = default);
	Task<PagedResult<Reminder>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken ct = default);
	Task<int> CountByUserAsync(int userId, CancellationToken ct);
	Task<int> ExecuteUpdateUserRemindAsync(int remindId, int userId, string textRemind, DateTime dateTime, CancellationToken ct = default);
}
