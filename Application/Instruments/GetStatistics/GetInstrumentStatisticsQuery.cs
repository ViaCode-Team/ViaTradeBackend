using Mediator;

namespace ViaTrade.Application.Instruments.GetStatistics;

public sealed record GetInstrumentStatisticsQuery() : IQuery<InstrumentStatisticsResult>;
