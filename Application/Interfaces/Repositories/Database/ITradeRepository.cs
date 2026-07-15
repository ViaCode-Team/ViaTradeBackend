using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;
using Domain.Interfaces;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeRepository : IRepository<Trade, TradeDto>
{
	Task<GlobalStatistic> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetPagedFilteredAsync(ISpecification<Trade> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
}
