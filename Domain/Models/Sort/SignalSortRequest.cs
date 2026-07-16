using Domain.Enums;

namespace Domain.Models.Sort;

public class SignalSortRequest
{
	public SignalSortOrder SortOrder { get; init; } = SignalSortOrder.NewestFirst;
}
