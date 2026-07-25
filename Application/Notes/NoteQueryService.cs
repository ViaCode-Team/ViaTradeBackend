using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Specifications;
using Application.Notes.Interfaces;
using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes;

public class NoteQueryService(INoteRepository noteRepository) : INoteQueryService
{
	public async Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		return await noteRepository.GetStatisticsAsync(userId, ct);
	}

	public async Task<Note> GetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct)
	{
		var existingNote = await noteRepository.FindByTargetAsync(userId, relatedId, noteType, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
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
		TradeCodeBriefDto? tradeCode = null;
		if (source.TradeCodeId.HasValue)
			tradeCode = new TradeCodeBriefDto(source.TradeCodeId.Value, source.TradeCodeTicker!, source.TradeCodeName);

		StrategyBriefDto? strategy = null;
		if (source.StrategyId.HasValue)
			strategy = new StrategyBriefDto(source.StrategyId.Value, source.StrategyName!, source.StrategyDescription);

		return new NoteDto(source.Id, source.NoteText, source.UserId, tradeCode, strategy);
	}
}
