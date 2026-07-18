using Domain.Reminds.Entities;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeRemindRepository : IRepository<TradeRemind>
{
	Task<IEnumerable<TradeRemind>> GetActualRemind(CancellationToken cancellationToken);
	Task<PagedResult<TradeRemind>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeRemind>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken);
	Task<int> UpdateUserRemindAsync(int remindId, int userId, string textRemind, DateTime dateTime, CancellationToken cancellationToken = default);
}
