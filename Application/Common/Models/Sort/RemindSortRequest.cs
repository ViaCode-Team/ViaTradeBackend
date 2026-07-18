using Domain.Reminds.Enums;
namespace Application.Common.Models.Sort;

public record RemindSortRequest() : BaseSortRequest<RemindSortField>
{
	public RemindSortRequest(bool _) : this()
	{
		SortBy = [RemindSortField.DateTimeDesc];
	}
}
