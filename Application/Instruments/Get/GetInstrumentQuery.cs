using Mediator;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Instruments.Get;

public sealed record GetInstrumentQuery(int InstrumentId) : IQuery<InstrumentResult>;
