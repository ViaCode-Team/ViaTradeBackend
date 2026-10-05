using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Instruments.GetBySymbol;

public sealed record GetInstrumentBySymbolQuery(string Symbol) : IQuery<InstrumentResult>;
