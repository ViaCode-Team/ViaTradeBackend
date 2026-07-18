using Application.Common.Interfaces.Repositories;
using Application.Statistics.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteRepository : IRepository<Note>
{
	Task<NoteStatisticReadModel> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken = default);
	Task<Note> GetUserNoteByProp(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task AddUserNoteAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken cancellationToken);
	Task UpdateUserNoteAsync(int id, NoteType noteType, int userId, string noteText, CancellationToken cancellationToken);
	Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task<Note?> FindUserNoteByEntityAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken);
}
