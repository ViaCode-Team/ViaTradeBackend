using Application.Common.Interfaces.Repositories;
using Application.Common.Queries;
using Application.TradeCodes.Queries;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeRepository : IRepository<TradeCode>
{
	Task<int> CountAsync(CancellationToken ct = default);
	Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken ct = default);
	Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken ct = default);
	Task<PageResult<TradeCode>> GetCodesPagedAsync(
		PageOptions page,
		TradeCodeSort sort,
		CancellationToken ct = default
	);
}
