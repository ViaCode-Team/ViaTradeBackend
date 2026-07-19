using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeRepository : IRepository<TradeCode>
{
	Task<int> CountAsync(CancellationToken ct = default);
	Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken ct = default);
	Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken ct = default);
	Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest sortRequest, CancellationToken ct = default);
}
