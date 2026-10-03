using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Notes.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes;

public class NoteQueryService(
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<Strategy> strategyRepository,
	IReadRepository<Note> noteRepository,
	INoteRepository noteStatistics
) : INoteQueryService
{
	public async Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		return await noteStatistics.GetStatisticsAsync(userId, ct);
	}

	public async Task<Note> GetByIdAsync(int userId, int noteId, CancellationToken ct)
	{
		var specification = new NoteWithTargetsSpecification(userId, note => note.Id == noteId);
		return await noteRepository.FirstOrDefaultAsync(specification, ct)
			?? throw new NotFoundException("Note not found.", "note_not_found");
	}

	public async Task<Note> GetInstrumentAsync(int userId, int instrumentId, CancellationToken ct)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(instrument => instrument.Id == instrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var specification = new NoteWithTargetsSpecification(userId, note => note.InstrumentId == instrumentId);
		var existingNote = await noteRepository.FirstOrDefaultAsync(specification, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
	}

	public async Task<Note> GetStrategyAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == strategyId, ct);

		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var specification = new NoteWithTargetsSpecification(userId, note => note.StrategyId == strategyId);
		var existingNote = await noteRepository.FirstOrDefaultAsync(specification, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
	}

	public async Task<PageResult<NoteDto>> GetPageAsync(
		int userId,
		NoteFilter noteFilter,
		NoteSearch noteSearch,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var specification = new NotesPageSpecification(userId, noteFilter, noteSearch, pageOptions);
		var notes = await PageQuery.ExecuteAsync(
			noteRepository,
			specification,
			note => new NoteProjectionDto(
				note.Id,
				note.Text,
				note.UserId,
				note.InstrumentId,
				note.Instrument!.Symbol,
				note.Instrument.Description,
				note.StrategyId,
				note.Strategy!.Name,
				note.Strategy.DisplayName,
				note.Strategy.Description
			),
			ct
		);

		return notes.Map(ToDto);
	}

	private static NoteDto ToDto(NoteProjectionDto source)
	{
		InstrumentBriefDto? instrument = null;
		if (source.InstrumentId.HasValue)
			instrument = new InstrumentBriefDto(
				source.InstrumentId.Value,
				source.InstrumentTicker!,
				source.InstrumentName
			);

		StrategyBriefDto? strategy = null;
		if (source.StrategyId.HasValue)
			strategy = new StrategyBriefDto(
				source.StrategyId.Value,
				source.StrategyName!,
				source.StrategyDisplayName!,
				source.StrategyDescription
			);

		return new NoteDto(source.Id, source.Text, source.UserId, instrument, strategy);
	}
}
