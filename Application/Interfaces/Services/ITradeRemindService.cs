using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;

namespace Application.Interfaces
{
    public interface ITradeRemindService
    {
        Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken);
        Task<TradeRemindStatistic> GetTradeRemindStatisticAsync(int userId, CancellationToken cancellationToken);
        Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken);
        Task<PagedResult<TradeRemind>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<PagedResult<TradeRemind>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken);
        Task CreateAsync(int userId, int tradeCodeId, TradeRemindRequest request, CancellationToken cancellationToken);
        Task UpdateAsync(int remindId, int userId, TradeRemindRequest request, CancellationToken cancellationToken);
        Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken);
    }
}
