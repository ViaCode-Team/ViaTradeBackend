using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Get;

public sealed class GetStrategyHandler(IReadRepository<Strategy> strategyRepository)
	: IQueryHandler<GetStrategyQuery, StrategySubscriptionResult>
{
	public async Task<StrategySubscriptionResult> HandleAsync(GetStrategyQuery query, CancellationToken ct = default)
	{
		var strategy = await strategyRepository.FirstOrDefaultAsync(
			strategy => strategy.Id == query.StrategyId,
			StrategySubscriptionResult.Projection(query.UserId),
			ct
		);
		if (strategy == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		return strategy;
	}
}
