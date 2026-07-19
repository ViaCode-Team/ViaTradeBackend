using Application.Common.Queries;
using Application.Common.Specifications;
using Application.Notes.Interfaces;
using Application.Notes.Queries;
using Application.Statistics.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes;

public class NoteQueryService(INoteRepository noteRepository) : INoteQueryService
{
	public async Task<NoteStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		return await noteRepository.GetNoteStatisticAsync(userId, ct);
	}

	public async Task<Note> GetAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct)
	{
		var existingNotes = await noteRepository.FindAsync(
			x =>
				x.UserId == userId
				&& (noteType == NoteType.TradeCodeNote ? x.TradeCodeId == relatedId : x.TradeStrategyId == relatedId),
			ct
		);

		var existingNote = existingNotes.FirstOrDefault();

		if (existingNote == null)
		{
			throw new Exception("Note not found.");
		}

		return existingNote;
	}

	public async Task<PageResult<Note>> GetAsync(int userId, NoteFilter filter, PageOptions page, CancellationToken ct)
	{
		var spec = new NoteQuerySpecification(userId, filter);
		return await noteRepository.GetPagedAsync(spec, page, ct);
	}
}
