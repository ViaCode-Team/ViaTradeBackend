using Domain.Trades.Enums;
using System.ComponentModel;

namespace Application.Common.Models.Sort;

public record StockSortRequest() : BaseSortRequest<StockSortField>
{
	[DefaultValue(StockSortField.NameAsc)]
	protected override List<StockSortField> DefaultSortBy => [StockSortField.NameAsc];
}
