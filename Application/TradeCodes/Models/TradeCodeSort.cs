using Application.Common.Models;

namespace Application.TradeCodes.Models;

public record TradeCodeSort() : Sort<TradeCodeSortField>
{
	protected override List<TradeCodeSortField> DefaultSortBy => [TradeCodeSortField.NameAsc];
}
