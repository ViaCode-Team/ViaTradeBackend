using Application.Common.Models;
using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteQueryService
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Note> GetByIdAsync(int userId, int noteId, CancellationToken ct);
	Task<Note> GetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct);
	Task<PageResult<NoteDto>> GetPageAsync(
		int userId,
		NoteFilter noteFilter,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
