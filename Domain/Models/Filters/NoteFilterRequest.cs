using Domain.Notes.Enums;
using Domain.Entities.DataBase;

namespace Domain.Models.Filters;

public record NoteFilterRequest(
	NoteType? Target
);
