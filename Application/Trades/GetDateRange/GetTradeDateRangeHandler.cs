using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetDateRange;

public sealed class GetTradeDateRangeHandler(IUserContext userContext, ITradeRepository tradeStatistics)
	: IQueryHandler<GetTradeDateRangeQuery, TradeDateRangeResult>
{
	public ValueTask<TradeDateRangeResult> Handle(GetTradeDateRangeQuery query, CancellationToken ct)
	{
		return new(tradeStatistics.GetTradeDateRangeAsync(userContext.UserId, ct));
	}
}
