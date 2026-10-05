using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.UpsertStrategy;

public sealed class UpsertStrategyNoteHandler(
	IRepository<Note> noteRepository,
	INoteRepository noteOperations,
	IUnitOfWork uow
) : ICommandHandler<UpsertStrategyNoteCommand>
{
	public async Task HandleAsync(UpsertStrategyNoteCommand command, CancellationToken ct = default)
	{
		int affectedRows = await noteOperations.ExecuteUpdateStrategyAsync(command.UserId, command.StrategyId, command.Text, ct);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = command.UserId,
			Text = command.Text,
			StrategyId = command.StrategyId,
		};

		noteRepository.Add(note);
		await uow.SaveChangesAsync(ct);
	}
}
