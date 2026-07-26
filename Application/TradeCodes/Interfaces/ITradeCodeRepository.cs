using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.TradeCodes.Models;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeRepository : IRepository<TradeCode>
{
	Task<TradeCode?> FindByTickerAsync(string ticker, CancellationToken ct = default);
	Task<int?> FindIdByTickerAsync(string ticker, CancellationToken ct = default);
	Task<string?> FindTickerByIdAsync(int tradeCodeId, CancellationToken ct = default);
	Task<Dictionary<string, int>> GetTradeCodeIdByTickerAsync(CancellationToken ct = default);
	Task<PageResult<TradeCode>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct = default
	);
}
