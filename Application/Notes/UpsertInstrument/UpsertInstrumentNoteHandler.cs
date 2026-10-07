using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.UpsertInstrument;

public sealed class UpsertInstrumentNoteHandler(
	IUserContext userContext,
	IRepository<Note> noteRepository,
	INoteRepository noteOperations,
	IUnitOfWork uow
) : ICommandHandler<UpsertInstrumentNoteCommand>
{
	public async Task HandleAsync(UpsertInstrumentNoteCommand command, CancellationToken ct)
	{
		int affectedRows = await noteOperations.ExecuteUpdateInstrumentAsync(
			userContext.UserId,
			command.InstrumentId,
			command.Text,
			ct
		);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = userContext.UserId,
			Text = command.Text,
			InstrumentId = command.InstrumentId,
		};

		noteRepository.Add(note);
		await uow.SaveChangesAsync(ct);
	}
}
