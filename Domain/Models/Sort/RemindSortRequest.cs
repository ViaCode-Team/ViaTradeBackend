using Domain.Enums;

namespace Domain.Models.Sort;

public class RemindSortRequest
{
	public RemindSortOrder SortOrder { get; init; } = RemindSortOrder.NewestFirst;
}
