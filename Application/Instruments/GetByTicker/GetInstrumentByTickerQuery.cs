using Mediator;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Instruments.GetByTicker;

public sealed record GetInstrumentByTickerQuery(string Ticker) : IQuery<InstrumentResult>;
