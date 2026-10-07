using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.GetPage;

public sealed class GetStrategiesPageHandler(IUserContext userContext, IReadRepository<Strategy> strategyRepository)
	: IQueryHandler<GetStrategiesPageQuery, PageResult<StrategySubscriptionResult>>
{
	public async ValueTask<PageResult<StrategySubscriptionResult>> Handle(
		GetStrategiesPageQuery query,
		CancellationToken ct
	)
	{
		var specification = new StrategiesPageSpecification(
			query.StrategyFilter,
			query.StrategySearch,
			query.PageOptions,
			query.StrategySort
		);

		return await strategyRepository.GetPageAsync(
			specification,
			StrategySubscriptionResult.Projection(userContext.UserId),
			ct
		);
	}
}
