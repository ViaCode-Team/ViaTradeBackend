using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Get;

public sealed class GetStrategyHandler(IUserContext userContext, IReadRepository<Strategy> strategyRepository)
	: IQueryHandler<GetStrategyQuery, StrategySubscriptionResult>
{
	public async ValueTask<StrategySubscriptionResult> Handle(GetStrategyQuery query, CancellationToken ct)
	{
		var strategy = await strategyRepository.FirstOrDefaultAsync(
			strategy => strategy.Id == query.StrategyId,
			StrategySubscriptionResult.Projection(userContext.UserId),
			ct
		);
		if (strategy == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		return strategy;
	}
}
