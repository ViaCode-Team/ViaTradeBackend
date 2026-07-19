using Application.Common.Queries;
using Application.Notes.Queries;
using Application.Statistics.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteQueryService
{
	Task<NoteStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Note> GetAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct);
	Task<PageResult<Note>> GetAsync(int userId, NoteFilter filter, PageOptions page, CancellationToken ct);
}
