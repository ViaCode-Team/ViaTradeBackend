using Mediator;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.GetInstrument;

public sealed record GetInstrumentNoteQuery(int InstrumentId) : IQuery<NoteResult>;
