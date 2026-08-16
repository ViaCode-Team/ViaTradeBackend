using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Trades.Models;

public record SignalSort() : Sort<SignalSortField>
{
	protected override List<SignalSortField> DefaultSortBy => [SignalSortField.SignalDateDesc];
}
