using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeRepository : IRepository<TradeCode>
{
	Task<int> CountAsync(CancellationToken cancellationToken = default);
	Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
	Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
}
