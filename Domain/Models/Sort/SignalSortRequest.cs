using Domain.Enums;

namespace Domain.Models.Sort;

public record SignalSortRequest() : BaseSortRequest<SignalSortField>
{
	public SignalSortRequest(bool _) : this()
	{
		SortBy = [SignalSortField.DateTimeDesc];
	}
}
