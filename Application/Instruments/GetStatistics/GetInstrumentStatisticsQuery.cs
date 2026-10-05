using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Instruments.GetStatistics;

public sealed record GetInstrumentStatisticsQuery() : IQuery<InstrumentStatisticsResult>;
