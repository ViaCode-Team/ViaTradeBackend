using Mediator;

namespace ViaTrade.Application.Strategies.LinkInstrument;

public sealed record LinkStrategyInstrumentCommand(int StrategyId, int InstrumentId) : ICommand;
