using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.GetPage;

public sealed class GetTradesPageHandler(IReadRepository<Trade> tradeRepository)
	: IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>>
{
	public async Task<PageResult<TradeResult>> HandleAsync(GetTradesPageQuery query, CancellationToken ct = default)
	{
		var specification = new TradesPageSpecification(
			query.UserId,
			query.TradeFilter,
			query.TradeSearch,
			query.PageOptions
		);
		return await tradeRepository.GetPageAsync(specification, TradeResult.Projection, ct);
	}
}
