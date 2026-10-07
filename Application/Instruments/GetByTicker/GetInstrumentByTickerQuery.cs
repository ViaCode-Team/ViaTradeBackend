using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Instruments.GetByTicker;

public sealed record GetInstrumentByTickerQuery(string Ticker) : IQuery<InstrumentResult>;
