using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteRepository : IRepository<Note>
{
	Task<NoteStatisticDto> GetNoteStatisticAsync(int userId, CancellationToken ct = default);
	Task<Note?> FindByTargetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct = default);
	Task AddUserNoteAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct);
	Task<int> ExecuteUpdateUserNoteAsync(int id, NoteType noteType, int userId, string noteText, CancellationToken ct);
	Task<int> DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken ct);
}
