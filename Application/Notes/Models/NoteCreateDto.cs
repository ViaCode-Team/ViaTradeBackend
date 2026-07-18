using System.ComponentModel.DataAnnotations;

namespace Application.Notes.Models;

public record NoteCreateDto
{
	[StringLength(1024)]
	public required string NoteText { get; set; }
}
