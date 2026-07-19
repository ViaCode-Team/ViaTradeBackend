using Application.Common.Queries;

namespace Application.Trades.Queries;

public record SignalSort() : Sort<SignalSortField>
{
	protected override List<SignalSortField> DefaultSortBy => [SignalSortField.DateTimeDesc];
}
