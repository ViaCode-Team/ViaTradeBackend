using Application.Common.Queries;

namespace Application.TradeCodes.Queries;

public record TradeCodeSort() : Sort<TradeCodeSortField>
{
	protected override List<TradeCodeSortField> DefaultSortBy => [TradeCodeSortField.NameAsc];
}
