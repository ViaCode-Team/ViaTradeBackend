using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Strategies.GetStatistics;

public sealed record GetStrategyStatisticsQuery() : IQuery<StrategyStatisticsResult>;
