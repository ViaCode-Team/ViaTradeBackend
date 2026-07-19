using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Reminds.Models;
using Domain.Reminds.Entities;

namespace Application.Reminds.Interfaces;

public interface IRemindQueryService
{
	Task<RemindStatisticDto> GetStatistics(int userId, CancellationToken ct);
	Task<IEnumerable<Reminder>> GetActualAsync(CancellationToken ct);
	Task<Reminder> GetAsync(int remindId, int userId, CancellationToken ct);
	Task<PagedResult<Reminder>> GetPagedAsync(
		int userId,
		int tradeCodeId,
		PaginationRequest paginationRequest,
		RemindSortRequest? sortRequest,
		CancellationToken ct);
	Task<PagedResult<Reminder>> GetPagedAsync(
		int userId,
		PaginationRequest paginationRequest,
		RemindSortRequest? sortRequest,
		CancellationToken ct);
}
