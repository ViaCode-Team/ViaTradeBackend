using Domain.Notes.Enums;

namespace Application.Notes.Queries;

public record NoteFilter(
	NoteType? Target
);
