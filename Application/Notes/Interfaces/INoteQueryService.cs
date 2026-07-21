using Application.Common.Models;
using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteQueryService
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Note> GetAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct);
	Task<PageResult<Note>> GetAsync(int userId, NoteFilter filter, PageOptions page, CancellationToken ct);
}
