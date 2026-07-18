using Domain.Trades.Enums;
namespace Application.Common.Models.Sort;

public record StockSortRequest() : BaseSortRequest<StockSortField>
{
	public StockSortRequest(bool _) : this()
	{
		SortBy = [StockSortField.NameAsc];
	}
}
