using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;
using ViaTradeBackend.Models.Trade;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeRepository : IRepository<Trade, TradeDto>
{
	Task<GlobalStatistic> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<int> UpdateUserTradeAsync(int id, int userId, TradeRequest request, double? netIncome, decimal price, CancellationToken cancellationToken = default);
}
