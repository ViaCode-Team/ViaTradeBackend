using Domain.Notes.Enums;

namespace Application.Common.Models.Filters;

public record NoteFilterRequest(
	NoteType? Target
);
