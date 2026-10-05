using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Strategies.GetStatistics;

public sealed record GetStrategyStatisticsQuery(int UserId) : IQuery<StrategyStatisticsResult>;
