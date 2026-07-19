using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteCommandService
{
	Task AddAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct);
	Task DeleteAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct);
	Task UpdateAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct);
}
