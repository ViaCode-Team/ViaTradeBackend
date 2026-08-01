using Application.Common.Models;

namespace Application.Trades.Models;

public record SignalSort() : Sort<SignalSortField>
{
	protected override List<SignalSortField> DefaultSortBy => [SignalSortField.SignalDateDesc];
}
