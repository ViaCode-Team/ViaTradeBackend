using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Get;

public sealed class GetTradeHandler(IUserContext userContext, IReadRepository<Trade> tradeRepository)
	: IQueryHandler<GetTradeQuery, TradeResult>
{
	public async ValueTask<TradeResult> Handle(GetTradeQuery query, CancellationToken ct)
	{
		var trade = await tradeRepository.FirstOrDefaultAsync(
			trade => trade.UserId == userContext.UserId && trade.Id == query.TradeId,
			TradeResult.Projection,
			ct
		);
		if (trade == null)
			throw new NotFoundException("Trade not found.", "trade_not_found");

		return trade;
	}
}
