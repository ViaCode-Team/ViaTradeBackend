using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Signals.Common;

public record SignalSort() : Sort<SignalSortField>
{
	protected override List<SignalSortField> DefaultSortBy => [SignalSortField.SignalDateDesc];
}
