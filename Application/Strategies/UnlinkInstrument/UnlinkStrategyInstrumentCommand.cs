using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Strategies.UnlinkInstrument;

public sealed record UnlinkStrategyInstrumentCommand(int StrategyId, int InstrumentId) : ICommand;
