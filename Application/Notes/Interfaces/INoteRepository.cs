using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteRepository : IRepository<Note>
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct = default);
	Task<PageResult<NoteProjectionDto>> GetPageWithTargetsAsync(
		IQuerySpecification<Note> specification,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<Note?> FindByTargetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct = default);
	Task AddUserNoteAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct);
	Task<int> ExecuteUpdateUserNoteAsync(int userId, int id, NoteType noteType, string noteText, CancellationToken ct);
	Task<int> ExecuteDeleteUserNoteAsync(int userId, int id, NoteType noteType, CancellationToken ct);
}
