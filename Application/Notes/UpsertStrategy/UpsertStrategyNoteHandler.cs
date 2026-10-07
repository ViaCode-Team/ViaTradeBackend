using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.UpsertStrategy;

public sealed class UpsertStrategyNoteHandler(
	IUserContext userContext,
	IRepository<Note> noteRepository,
	INoteRepository noteOperations,
	IUnitOfWork uow
) : IVoidCommandHandler<UpsertStrategyNoteCommand>
{
	public async ValueTask Handle(UpsertStrategyNoteCommand command, CancellationToken ct)
	{
		int affectedRows = await noteOperations.ExecuteUpdateStrategyAsync(
			userContext.UserId,
			command.StrategyId,
			command.Text,
			ct
		);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = userContext.UserId,
			Text = command.Text,
			StrategyId = command.StrategyId,
		};

		noteRepository.Add(note);
		await uow.SaveChangesAsync(ct);
	}
}
