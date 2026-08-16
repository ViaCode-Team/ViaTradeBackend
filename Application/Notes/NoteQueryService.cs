using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Notes.Interfaces;
using Application.Notes.Models;
using Application.Notes.Specifications;
using Domain.Entities;
using Domain.Enums;

namespace Application.Notes;

public class NoteQueryService(INoteRepository noteRepository) : INoteQueryService
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

	public async Task<Note> GetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct)
	{
		var existingNote = await noteRepository.FindByTargetAsync(userId, relatedId, noteType, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
	}

	public async Task<PageResult<NoteDto>> GetSearchAsync(
		int userId,
		SearchFilter noteSearchFilter,
		PageOptions pageOptions,
		CancellationToken ct)
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
			strategy = new StrategyBriefDto(source.StrategyId.Value, source.StrategyName!, source.StrategyDescription);

		return new NoteDto(source.Id, source.Text, source.UserId, instrument, strategy);
	}
}
