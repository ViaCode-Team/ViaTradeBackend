using Domain.Trades.Enums;
namespace Application.Common.Models.Sort;

public record SignalSortRequest() : BaseSortRequest<SignalSortField>
{
	public SignalSortRequest(bool _) : this()
	{
		SortBy = [SignalSortField.DateTimeDesc];
	}
}
