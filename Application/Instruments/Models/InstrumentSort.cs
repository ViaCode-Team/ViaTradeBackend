using Application.Common.Models;

namespace Application.Instruments.Models;

public record InstrumentSort() : Sort<InstrumentSortField>
{
	protected override List<InstrumentSortField> DefaultSortBy => [InstrumentSortField.SymbolAsc];
}
