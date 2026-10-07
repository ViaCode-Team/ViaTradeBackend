using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Notes.UpsertInstrument;

public sealed record UpsertInstrumentNoteCommand(int InstrumentId, string Text) : ICommand;
