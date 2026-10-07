using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.DeleteInstrument;

public sealed class DeleteInstrumentNoteHandler(IUserContext userContext, IRepository<Note> noteRepository)
	: ICommandHandler<DeleteInstrumentNoteCommand>
{
	public async Task HandleAsync(DeleteInstrumentNoteCommand command, CancellationToken ct)
	{
		int affectedRows = await noteRepository.ExecuteDeleteAsync(
			note => note.UserId == userContext.UserId && note.InstrumentId == command.InstrumentId,
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}
}
