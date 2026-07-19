using Domain.Trades.Enums;
namespace Application.Common.Models.Sort;

public record StockSortRequest() : BaseSortRequest<StockSortField>
{
	protected override List<StockSortField> DefaultSortBy => [StockSortField.NameAsc];
}
