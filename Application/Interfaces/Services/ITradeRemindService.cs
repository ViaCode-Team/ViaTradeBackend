using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Interfaces;

public interface ITradeRemindService
{
	Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken);
	Task<TradeRemindStatistic> GetRemindStatisticAsync(int userId, CancellationToken cancellationToken);
	Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken);
	Task<PagedResult<TradeRemindDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeRemindDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken);
	Task CreateAsync(int userId, int tradeCodeId, TradeRemindRequest request, CancellationToken cancellationToken);
	Task UpdateAsync(int remindId, int userId, TradeRemindRequest request, CancellationToken cancellationToken);
	Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken);
}
