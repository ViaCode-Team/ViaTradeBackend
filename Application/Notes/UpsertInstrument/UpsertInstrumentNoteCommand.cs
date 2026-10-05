using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Notes.UpsertInstrument;

public sealed record UpsertInstrumentNoteCommand(int UserId, int InstrumentId, string Text) : ICommand;
