using Application.Common.Models;
using Application.Trades.Models;

namespace Application.Trades.Interfaces;

public interface ITradeQueryService
{
	Task<GlobalTradeStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<TradeDto> GetAsync(int userId, int id, CancellationToken ct);
	Task<PageResult<TradeDto>> GetPageAsync(
		int userId,
		TradeFilter tradeFilter,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
