using Domain.Entities.DataBase;
using Domain.Models.Pagination;

namespace Domain.Models.Filters;

public record NoteFilterRequest
{
	public NoteType? Target { get; init; }
}
