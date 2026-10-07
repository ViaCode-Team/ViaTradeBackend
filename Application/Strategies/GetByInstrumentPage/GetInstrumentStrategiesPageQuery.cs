using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetByInstrumentPage;

public sealed record GetInstrumentStrategiesPageQuery(
	int InstrumentId,
	StrategyFilter StrategyFilter,
	StrategySort StrategySort,
	PageOptions PageOptions
) : IQuery<PageResult<StrategySubscriptionResult>>;
