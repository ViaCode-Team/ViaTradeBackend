using Mediator;

namespace ViaTrade.Application.Notes.DeleteInstrument;

public sealed record DeleteInstrumentNoteCommand(int InstrumentId) : ICommand;
