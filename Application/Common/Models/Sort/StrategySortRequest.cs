using Domain.Strategies.Enums;

namespace Application.Common.Models.Sort;

public record StrategySortRequest() : BaseSortRequest<StrategySortField>
{
	protected override List<StrategySortField> DefaultSortBy => [StrategySortField.NameAsc];
}
