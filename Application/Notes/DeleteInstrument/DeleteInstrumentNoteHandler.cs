using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.DeleteInstrument;

public sealed class DeleteInstrumentNoteHandler(IRepository<Note> noteRepository)
	: ICommandHandler<DeleteInstrumentNoteCommand>
{
	public async Task HandleAsync(DeleteInstrumentNoteCommand command, CancellationToken ct = default)
	{
		int affectedRows = await noteRepository.ExecuteDeleteAsync(
			note => note.UserId == command.UserId && note.InstrumentId == command.InstrumentId,
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}
}
