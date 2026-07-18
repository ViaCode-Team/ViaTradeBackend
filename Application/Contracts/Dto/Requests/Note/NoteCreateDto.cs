using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Dto.Requests.Note;

public record NoteCreateDto
{
	[StringLength(1024)]
	public required string NoteText { get; set; }
}
