using Domain.Enums;

namespace Domain.Models.Sort;

public class StrategySortRequest : BaseSortRequest<StrategySortField>
{
	public StrategySortRequest()
	{
		SortBy = [StrategySortField.NameAsc];
	}
}
