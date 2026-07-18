using Domain.Enums;

namespace Domain.Models.Sort;

public record RemindSortRequest() : BaseSortRequest<RemindSortField>
{
	public RemindSortRequest(bool _) : this()
	{
		SortBy = [RemindSortField.DateTimeDesc];
	}
}
