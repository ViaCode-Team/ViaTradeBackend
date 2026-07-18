using Application.Contracts.Dto.NoteRemind;
using Application.Contracts.Dto.Statistic;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;
using Domain.Models.Sort;

using Application.Contracts.Dto.Requests.Remind;

namespace Application.Interfaces;
public interface ITradeRemindService
{
	Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken);
	Task<TradeRemindStatisticDto> GetRemindStatisticAsync(int userId, CancellationToken cancellationToken);
	Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken);
	Task<PagedResult<TradeRemindDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeRemindDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken);
	Task CreateAsync(int userId, int tradeCodeId, TradeRemindCreateDto request, CancellationToken cancellationToken);
	Task UpdateAsync(int remindId, int userId, TradeRemindCreateDto request, CancellationToken cancellationToken);
	Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken);
}
