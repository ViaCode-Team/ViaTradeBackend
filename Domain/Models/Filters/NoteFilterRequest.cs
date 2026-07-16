using Domain.Entities.DataBase;

namespace Domain.Models.Filters;

public class NoteFilterRequest
{
	public NoteType? Target { get; init; }
}
