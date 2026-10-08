using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.GetPage;

public sealed class GetTradesPageHandler(IUserContext userContext, IReadRepository<Trade> tradeRepository)
	: IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>>
{
	public async ValueTask<PageResult<TradeResult>> Handle(GetTradesPageQuery query, CancellationToken ct)
	{
		var specification = new TradesPageSpecification(
			userContext.UserId,
			query.TradeFilter,
			query.TradeSearch,
			query.PageOptions
		);

		return await tradeRepository.GetPageAsync(specification, TradeResult.Projection, ct);
	}
}
