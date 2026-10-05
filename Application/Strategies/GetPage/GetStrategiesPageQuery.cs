using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetPage;

public sealed record GetStrategiesPageQuery(
	int UserId,
	StrategyFilter StrategyFilter,
	StrategySearch StrategySearch,
	StrategySort StrategySort,
	PageOptions PageOptions
) : IQuery<PageResult<StrategySubscriptionResult>>;
