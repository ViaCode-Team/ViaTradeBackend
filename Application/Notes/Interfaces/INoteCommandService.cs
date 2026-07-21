using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteCommandService
{
	Task AddAsync(int userId, int relatedId, NoteType noteType, string noteText, CancellationToken ct);
	Task DeleteAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct);
	Task UpdateAsync(int userId, int relatedId, NoteType noteType, string noteText, CancellationToken ct);
}
