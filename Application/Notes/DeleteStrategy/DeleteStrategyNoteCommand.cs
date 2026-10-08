using Mediator;

namespace ViaTrade.Application.Notes.DeleteStrategy;

public sealed record DeleteStrategyNoteCommand(int StrategyId) : ICommand;
