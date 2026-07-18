using Application.Contracts.Dto.Trade;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeCodeRepository : IRepository<TradeCode>
{
	Task<int> CountAsync(CancellationToken cancellationToken = default);
	Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
	Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
}
