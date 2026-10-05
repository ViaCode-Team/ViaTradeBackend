using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetDateRange;

public sealed class GetTradeDateRangeHandler(ITradeRepository tradeStatistics)
	: IQueryHandler<GetTradeDateRangeQuery, TradeDateRangeResult>
{
	public Task<TradeDateRangeResult> HandleAsync(GetTradeDateRangeQuery query, CancellationToken ct = default)
	{
		return tradeStatistics.GetTradeDateRangeAsync(query.UserId, ct);
	}
}
