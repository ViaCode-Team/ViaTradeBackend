using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.Get;

public sealed record GetNoteQuery(int UserId, int NoteId) : IQuery<NoteResult>;
