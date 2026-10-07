using Mediator;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.Get;

public sealed record GetNoteQuery(int NoteId) : IQuery<NoteResult>;
