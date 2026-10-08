using Mediator;

namespace ViaTrade.Application.Signals.GetStatistics;

public sealed record GetSignalStatisticsQuery() : IQuery<SignalStatisticsResult>;
