using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.TradeCodes.Models;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeRepository : IRepository<TradeCode>
{
	Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken ct = default);
	Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken ct = default);
	Task<string?> GetExchangeIdByIdAsync(int id, CancellationToken ct = default);
	Task<Dictionary<string, int>> GetExchangeIdMapAsync(CancellationToken ct = default);
	Task<PageResult<TradeCode>> GetCodesPagedAsync(
		PageOptions page,
		TradeCodeSort sort,
		CancellationToken ct = default
	);
}
