using Mediator;

namespace ViaTrade.Application.Notes.UpsertStrategy;

public sealed record UpsertStrategyNoteCommand(int StrategyId, string Text) : ICommand;
