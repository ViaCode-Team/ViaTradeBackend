using Application.Common.Models;

namespace Application.TradeCodes.Models;

public record InstrumentSort() : Sort<InstrumentSortField>
{
	protected override List<InstrumentSortField> DefaultSortBy => [InstrumentSortField.SymbolAsc];
}
