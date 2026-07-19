using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Statistics.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteQueryService
{
	Task<NoteStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Note> GetAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct);
	Task<PagedResult<Note>> GetAsync(int userId, NoteFilterRequest filterRequest, PaginationRequest paginationRequest, CancellationToken ct);
}
