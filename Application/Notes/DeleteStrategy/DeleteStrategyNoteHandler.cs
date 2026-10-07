using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.DeleteStrategy;

public sealed class DeleteStrategyNoteHandler(IUserContext userContext, IRepository<Note> noteRepository)
	: ICommandHandler<DeleteStrategyNoteCommand>
{
	public async Task HandleAsync(DeleteStrategyNoteCommand command, CancellationToken ct)
	{
		int affectedRows = await noteRepository.ExecuteDeleteAsync(
			note => note.UserId == userContext.UserId && note.StrategyId == command.StrategyId,
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}
}
