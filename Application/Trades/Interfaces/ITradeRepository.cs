using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Queries;
using Application.Statistics.Models;
using Application.Trades.Models;
using Domain.Trades.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeRepository : IRepository<Trade>
{
	Task<GlobalStatisticReadModel> GetGlobalStatisticAsync(int userId, CancellationToken ct = default);
	Task<PageResult<Trade>> GetByUserPagedAsync(int userId, PageOptions page, CancellationToken ct = default);
	Task<PageResult<Trade>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PageOptions page, CancellationToken ct = default);
	Task<PageResult<Trade>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PageOptions page, CancellationToken ct = default);
	Task<int> UpdateAsync(int id, int userId, TradeInput request, double? netIncome, decimal price, CancellationToken ct = default);
}
