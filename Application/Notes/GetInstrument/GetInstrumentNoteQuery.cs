using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.GetInstrument;

public sealed record GetInstrumentNoteQuery(int UserId, int InstrumentId) : IQuery<NoteResult>;
