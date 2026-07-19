using Application.Common.Queries;
using Application.Statistics.Models;
using Application.Trades.Queries;
using Domain.Trades.Entities;

namespace Application.Trades.Interfaces;

public interface ITradeQueryService
{
	Task<GlobalStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Trade> GetAsync(int id, int userId, CancellationToken ct);
	Task<PageResult<Trade>> GetAsync(int userId, TradeFilter filter, PageOptions page, CancellationToken ct);
}
