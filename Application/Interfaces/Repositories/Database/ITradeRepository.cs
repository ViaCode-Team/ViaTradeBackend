using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database
{
    public interface ITradeRepository : IRepository<Trade, TradeDto>
    {
        Task<GlobalStatistic> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken);
        Task<PagedResult<Trade>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<PagedResult<Trade>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<PagedResult<Trade>> GetByUserAndDateRangePagedAsync(int userId, DateTime? from, DateTime? to, TradeSignal? tradeSignal, PaginationRequest paginationRequest, CancellationToken cancellationToken);
    }
}
