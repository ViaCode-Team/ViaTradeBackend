using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes;

public class NoteCommandService(IRepository<Note> noteRepository, INoteRepository noteOperations, IUnitOfWork uow)
	: INoteCommandService
{
	public async Task UpsertInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct)
	{
		int affectedRows = await noteOperations.ExecuteUpdateInstrumentAsync(userId, instrumentId, text, ct);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = userId,
			Text = text,
			InstrumentId = instrumentId,
		};

		noteRepository.Add(note);
		await uow.SaveChangesAsync(ct);
	}

	public async Task UpsertStrategyAsync(int userId, int strategyId, string text, CancellationToken ct)
	{
		int affectedRows = await noteOperations.ExecuteUpdateStrategyAsync(userId, strategyId, text, ct);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = userId,
			Text = text,
			StrategyId = strategyId,
		};

		noteRepository.Add(note);
		await uow.SaveChangesAsync(ct);
	}

	public async Task DeleteInstrumentAsync(int userId, int instrumentId, CancellationToken ct)
	{
		int affectedRows = await noteRepository.ExecuteDeleteAsync(
			note => note.UserId == userId && note.InstrumentId == instrumentId,
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}

	public async Task DeleteStrategyAsync(int userId, int strategyId, CancellationToken ct)
	{
		int affectedRows = await noteRepository.ExecuteDeleteAsync(
			note => note.UserId == userId && note.StrategyId == strategyId,
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}
}
