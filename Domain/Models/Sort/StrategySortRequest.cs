using Domain.Enums;

namespace Domain.Models.Sort;

public class StrategySortRequest
{
	public StrategySortOrder SortOrder { get; init; } = StrategySortOrder.NameAscending;
}
