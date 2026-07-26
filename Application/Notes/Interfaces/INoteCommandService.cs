using Domain.Notes.Enums;

namespace Application.Notes.Interfaces;

public interface INoteCommandService
{
	Task DeleteAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct);
	Task UpsertAsync(int userId, int relatedId, NoteType noteType, string noteText, CancellationToken ct);
}
