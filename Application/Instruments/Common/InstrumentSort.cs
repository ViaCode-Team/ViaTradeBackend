using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Instruments.Common;

public record InstrumentSort() : Sort<InstrumentSortField>
{
	protected override List<InstrumentSortField> DefaultSortBy => [InstrumentSortField.SymbolAsc];
}
