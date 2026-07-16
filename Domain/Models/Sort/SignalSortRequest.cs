using Domain.Enums;

namespace Domain.Models.Sort;

public class SignalSortRequest : BaseSortRequest<SignalSortField>
{
	public SignalSortRequest()
	{
		SortBy = [SignalSortField.DateTimeDesc];
	}
}
