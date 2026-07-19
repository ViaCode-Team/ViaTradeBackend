using Domain.Trades.Enums;
namespace Application.Common.Models.Sort;

public record SignalSortRequest() : BaseSortRequest<SignalSortField>
{
	protected override List<SignalSortField> DefaultSortBy => [SignalSortField.DateTimeDesc];
}
