using Mediator;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.GetStrategy;

public sealed record GetStrategyNoteQuery(int StrategyId) : IQuery<NoteResult>;
