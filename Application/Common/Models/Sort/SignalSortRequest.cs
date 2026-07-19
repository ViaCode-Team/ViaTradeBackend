using Domain.Trades.Enums;
using System.ComponentModel;

namespace Application.Common.Models.Sort;

public record SignalSortRequest() : BaseSortRequest<SignalSortField>
{
	[DefaultValue(SignalSortField.DateTimeDesc)]
	protected override List<SignalSortField> DefaultSortBy => [SignalSortField.DateTimeDesc];
}
