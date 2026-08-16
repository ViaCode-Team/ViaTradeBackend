using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Instruments.Interfaces;
using Application.Notes.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Entities;

namespace Application.Notes;

public class NoteCommandService(
	IInstrumentRepository instrumentRepository,
	IStrategyRepository strategyRepository,
	INoteRepository noteRepository,
	IUnitOfWork uow
) : INoteCommandService
{
	public async Task UpsertInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct)
	{
		int affectedRows = await noteRepository.ExecuteUpdateInstrumentAsync(userId, instrumentId, text, ct);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = userId,
			Text = text,
			InstrumentId = instrumentId,
		};

		await SaveAsync(note, ct);
	}

	public async Task UpsertStrategyAsync(int userId, int strategyId, string text, CancellationToken ct)
	{
		int affectedRows = await noteRepository.ExecuteUpdateStrategyAsync(userId, strategyId, text, ct);
		if (affectedRows != 0)
			return;

		var note = new Note
		{
			UserId = userId,
			Text = text,
			StrategyId = strategyId,
		};

		await SaveAsync(note, ct);
	}

	public async Task DeleteInstrumentAsync(int userId, int instrumentId, CancellationToken ct)
	{
		var instrumentExists = await instrumentRepository.ExistsAsync(
			instrument => instrument.Id == instrumentId,
			ct
		);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		int affectedRows = await noteRepository.ExecuteDeleteInstrumentAsync(userId, instrumentId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}

	public async Task DeleteStrategyAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategyExists = await strategyRepository.ExistsAsync(strategy => strategy.Id == strategyId, ct);

		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		int affectedRows = await noteRepository.ExecuteDeleteStrategyAsync(userId, strategyId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}

	private async Task SaveAsync(Note note, CancellationToken ct)
	{
		await noteRepository.AddAsync(note, ct);
		await uow.SaveChangesAsync(ct);
	}
}
