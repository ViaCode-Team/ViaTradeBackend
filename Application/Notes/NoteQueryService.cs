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
		return await noteRepository.GetNoteStatisticAsync(userId, ct);
	}

	public async Task<Note> GetAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct)
	{
		var existingNote = await noteRepository.FindByTargetAsync(userId, relatedId, noteType, ct);
		if (existingNote == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return existingNote;
	}

	public async Task<PageResult<Note>> GetAsync(int userId, NoteFilter filter, PageOptions page, CancellationToken ct)
	{
		var spec = new NoteQuerySpecification(userId, filter);
		return await noteRepository.GetPagedAsync(spec, page, ct);
	}
}
