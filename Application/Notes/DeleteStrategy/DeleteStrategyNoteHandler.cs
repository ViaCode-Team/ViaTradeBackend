using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.DeleteStrategy;

public sealed class DeleteStrategyNoteHandler(IRepository<Note> noteRepository)
	: ICommandHandler<DeleteStrategyNoteCommand>
{
	public async Task HandleAsync(DeleteStrategyNoteCommand command, CancellationToken ct = default)
	{
		int affectedRows = await noteRepository.ExecuteDeleteAsync(
			note => note.UserId == command.UserId && note.StrategyId == command.StrategyId,
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}
}
