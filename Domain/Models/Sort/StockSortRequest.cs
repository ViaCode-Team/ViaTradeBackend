using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Models.Sort;

public class StockSortRequest : BaseSortRequest<StockSortField>
{
	public StockSortRequest()
	{
		SortBy = [StockSortField.NameAsc];
	}
}
