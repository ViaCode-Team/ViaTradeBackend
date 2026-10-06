using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.GetPage;

public sealed class GetStrategiesPageHandler(IReadRepository<Strategy> strategyRepository)
	: IQueryHandler<GetStrategiesPageQuery, PageResult<StrategySubscriptionResult>>
{
	public async Task<PageResult<StrategySubscriptionResult>> HandleAsync(
		GetStrategiesPageQuery query,
		CancellationToken ct = default
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
			StrategySubscriptionResult.Projection(query.UserId),
			ct
		);
	}
}
