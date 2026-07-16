using Domain.Enums;

namespace Domain.Models.Sort;

public class RemindSortRequest : BaseSortRequest<RemindSortField>
{
	public RemindSortRequest()
	{
		SortBy = [RemindSortField.DateTimeDesc];
	}
}
