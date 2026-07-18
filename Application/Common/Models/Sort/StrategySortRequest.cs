using Domain.Strategies.Enums;

namespace Application.Common.Models.Sort;

public record StrategySortRequest() : BaseSortRequest<StrategySortField>
{
	public StrategySortRequest(bool _) : this()
	{
		SortBy = [StrategySortField.NameAsc];
	}
}
