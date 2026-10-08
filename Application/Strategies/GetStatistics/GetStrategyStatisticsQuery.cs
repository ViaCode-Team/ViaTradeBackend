using Mediator;

namespace ViaTrade.Application.Strategies.GetStatistics;

public sealed record GetStrategyStatisticsQuery() : IQuery<StrategyStatisticsResult>;
