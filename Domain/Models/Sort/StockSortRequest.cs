using Domain.Enums;

namespace Domain.Models.Sort;

public class StockSortRequest
{
	public StockSortOrder SortOrder { get; init; } = StockSortOrder.NameAscending;
}
