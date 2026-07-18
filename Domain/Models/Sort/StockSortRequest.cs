using Domain.Enums;

namespace Domain.Models.Sort;

public record StockSortRequest() : BaseSortRequest<StockSortField>
{
	public StockSortRequest(bool _) : this()
	{
		SortBy = [StockSortField.NameAsc];
	}
}
