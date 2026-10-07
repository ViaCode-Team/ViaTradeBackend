using Mediator;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetPage;

public sealed record GetStrategiesPageQuery(
	StrategyFilter StrategyFilter,
	StrategySearch StrategySearch,
	StrategySort StrategySort,
	PageOptions PageOptions
) : IQuery<PageResult<StrategySubscriptionResult>>;
