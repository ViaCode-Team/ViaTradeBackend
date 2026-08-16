using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Notes.Specifications;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes;

public class NoteQueryService(
	IInstrumentRepository instrumentRepository,
	IStrategyRepository strategyRepository,
	INoteRepository noteRepository
) : INoteQueryService
{
	public async Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		return await noteRepository.GetStatisticsAsync(userId, ct);
	}

	public async Task<Note> GetByIdAsync(int userId, int noteId, CancellationToken ct)
	{
		return await noteRepository.FindByIdForUserAsync(userId, noteId, ct)
			?? throw new NotFoundException("Note not found.", "note_not_found");
	}

	public async Task<Note> GetInstrumentAsync(int userId, int instrumentId, CancellationToken ct)
	{
		var instrumentExists = await instrumentRepository.ExistsAsync(instrument => instrument.Id == instrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var existingNote = await noteRepository.FindByInstrumentAsync(userId, instrumentId, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
	}

	public async Task<Note> GetStrategyAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategyExists = await strategyRepository.ExistsAsync(strategy => strategy.Id == strategyId, ct);

		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var existingNote = await noteRepository.FindByStrategyAsync(userId, strategyId, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
	}

	public async Task<PageResult<NoteDto>> GetSearchAsync(
		int userId,
		NoteSearchFilter noteSearchFilter,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new NoteSearchSpecification(userId, noteSearchFilter);
		var notes = await noteRepository.GetPageSearchAsync(spec, pageOptions, ct);

		return notes.Map(ToDto);
	}

	public async Task<PageResult<NoteDto>> GetPageAsync(
		int userId,
		NoteFilter noteFilter,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new NoteQuerySpecification(userId, noteFilter);
		var notes = await noteRepository.GetPageWithTargetsAsync(spec, pageOptions, ct);

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
