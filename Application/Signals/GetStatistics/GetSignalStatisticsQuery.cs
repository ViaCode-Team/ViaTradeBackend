using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Signals.GetStatistics;

public sealed record GetSignalStatisticsQuery() : IQuery<SignalStatisticsResult>;
