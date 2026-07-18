using Domain.Enums;

namespace Domain.Models.Sort;

public record StrategySortRequest() : BaseSortRequest<StrategySortField>
{
	public StrategySortRequest(bool _) : this()
	{
		SortBy = [StrategySortField.NameAsc];
	}
}
